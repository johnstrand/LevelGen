namespace LevelGen.Internal;

internal static class GeneratorCore
{
    private static readonly Direction[] AllDirections = Enum.GetValues<Direction>();

    public static GenerationResult Generate(PrefabSet prefabSet, GenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(prefabSet);
        ArgumentNullException.ThrowIfNull(options);

        var roomVariants = prefabSet
            .SelectMany(prefab => PrefabVariantFactory.CreateVariants(prefab, options.AllowMirrorTransforms))
            .ToArray();

        if (roomVariants.Length == 0)
        {
            throw new InvalidOperationException("The prefab set did not produce any usable variants.");
        }

        var corridorVariants = options.AllowGeneratedCorridors
            ? CorridorPrefabFactory.CreateGeneratedCorridors(options.MaxCorridorLength)
                .SelectMany(prefab => PrefabVariantFactory.CreateVariants(prefab, true))
                .ToArray()
            : [];

        var targetRoomPlacements = Math.Max(1, options.MaxPrefabCount ?? Math.Clamp(prefabSet.Count, 1, 10));
        var random = new Random(options.Seed);

        for (var attempt = 0; attempt < 12; attempt++)
        {
            var seed = roomVariants[random.Next(roomVariants.Length)];
            var state = new LayoutState();
            AddPlacement(state, seed, new Point2(0, 0), isCorridor: false, [], []);

            if (TryExpand(state, roomVariants, corridorVariants, targetRoomPlacements, options, random, depth: 0, out var result))
            {
                return result;
            }
        }

        throw new InvalidOperationException("Unable to generate a valid contiguous level from the supplied prefabs.");
    }

    private static bool TryExpand(
        LayoutState state,
        IReadOnlyList<PrefabVariant> roomVariants,
        IReadOnlyList<PrefabVariant> corridorVariants,
        int targetRoomPlacements,
        GenerationOptions options,
        Random random,
        int depth,
        out GenerationResult result)
    {
        if (depth > 128)
        {
            result = GenerationResult.Empty;
            return false;
        }

        if ((state.RoomPlacementCount >= targetRoomPlacements && TryFinalize(state, out result)) ||
            (state.OpenConnectors.Count == 0 && TryFinalize(state, out result)))
        {
            return true;
        }

        var openConnector = ChooseNextConnector(state, roomVariants, corridorVariants, options);
        if (openConnector is null)
        {
            result = GenerationResult.Empty;
            return TryFinalize(state, out result);
        }

        var selectedConnector = openConnector.Value;

        var roomCandidates = BuildCandidates(state, selectedConnector, roomVariants, options, isCorridor: false);
        var corridorCandidates = Array.Empty<CandidatePlacement>();

        var roomCandidateCount = roomCandidates.Count;
        var canUseCorridors =
            options.AllowGeneratedCorridors &&
            corridorVariants.Count > 0 &&
            state.CorridorPlacementCount < Math.Max(1, targetRoomPlacements * 2) &&
            (roomCandidateCount == 0 || random.NextDouble() < 0.35);

        if (canUseCorridors)
        {
            corridorCandidates = [.. BuildCandidates(state, selectedConnector, corridorVariants, options, isCorridor: true)];
        }

        var orderedCandidates = new List<CandidatePlacement>(roomCandidates.Count + corridorCandidates.Length);
        orderedCandidates.AddRange(roomCandidates);
        orderedCandidates.AddRange(corridorCandidates);
        ShuffleInPlace(orderedCandidates, random);

        foreach (var candidate in orderedCandidates)
        {
            var nextState = state.Clone();
            AddPlacement(
                nextState,
                candidate.Variant,
                candidate.Origin,
                candidate.IsCorridor,
                candidate.LinkedExistingConnectorPositions,
                candidate.LinkedCandidateConnectorPositions);

            if (TryExpand(nextState, roomVariants, corridorVariants, targetRoomPlacements, options, random, depth + 1, out result))
            {
                return true;
            }
        }

        if (state.OpenConnectors.Remove(selectedConnector.Position) &&
            TryExpand(state, roomVariants, corridorVariants, targetRoomPlacements, options, random, depth + 1, out result))
        {
            return true;
        }

        state.OpenConnectors[selectedConnector.Position] = selectedConnector;
        result = GenerationResult.Empty;
        return false;
    }

    private static OpenConnector? ChooseNextConnector(
        LayoutState state,
        IReadOnlyList<PrefabVariant> roomVariants,
        IReadOnlyList<PrefabVariant> corridorVariants,
        GenerationOptions options)
    {
        OpenConnector? bestConnector = null;
        var bestScore = int.MaxValue;

        foreach (var connector in state.OpenConnectors.Values)
        {
            var score = CountCandidates(state, connector, roomVariants, options);
            if (score == 0 && options.AllowGeneratedCorridors)
            {
                score = CountCandidates(state, connector, corridorVariants, options);
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestConnector = connector;
            }
        }

        return bestConnector;
    }

    private static int CountCandidates(
        LayoutState state,
        OpenConnector connector,
        IReadOnlyList<PrefabVariant> variants,
        GenerationOptions options)
    {
        var count = 0;
        foreach (var variant in variants)
        {
            foreach (var connection in variant.Connections)
            {
                if (connection.Facing != connector.Facing.Opposite())
                {
                    continue;
                }

                var origin = connector.Position + connector.Facing.Offset() - connection.Position;
                if (TryValidatePlacement(state, variant, origin, connector, options, out _))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static List<CandidatePlacement> BuildCandidates(
        LayoutState state,
        OpenConnector openConnector,
        IReadOnlyList<PrefabVariant> variants,
        GenerationOptions options,
        bool isCorridor)
    {
        var candidates = new List<CandidatePlacement>();
        foreach (var variant in variants)
        {
            foreach (var connection in variant.Connections)
            {
                if (connection.Facing != openConnector.Facing.Opposite())
                {
                    continue;
                }

                var origin = openConnector.Position + openConnector.Facing.Offset() - connection.Position;
                if (TryValidatePlacement(state, variant, origin, openConnector, options, out var candidate))
                {
                    candidates.Add(candidate with { IsCorridor = isCorridor });
                }
            }
        }

        return candidates;
    }

    private static bool TryValidatePlacement(
        LayoutState state,
        PrefabVariant variant,
        Point2 origin,
        OpenConnector requiredConnection,
        GenerationOptions options,
        out CandidatePlacement candidate)
    {
        var localConnections = variant.Connections.ToDictionary(connection => connection.Position);
        var linkedExisting = new HashSet<Point2>();
        var linkedCandidate = new HashSet<Point2>();

        for (var y = 0; y < variant.Height; y++)
        {
            for (var x = 0; x < variant.Width; x++)
            {
                var tile = variant.Tiles[(y * variant.Width) + x];
                if (tile == TileKind.Empty)
                {
                    continue;
                }

                var worldPosition = origin + new Point2(x, y);
                if (state.OccupiedTiles.ContainsKey(worldPosition))
                {
                    candidate = default;
                    return false;
                }

                foreach (var direction in AllDirections)
                {
                    var neighborPosition = worldPosition + direction.Offset();
                    if (!state.OccupiedTiles.TryGetValue(neighborPosition, out var existingTile) ||
                        !tile.IsWalkable() ||
                        !existingTile.IsWalkable())
                    {
                        continue;
                    }

                    if (!localConnections.TryGetValue(new Point2(x, y), out var localConnection) ||
                        !state.OpenConnectors.TryGetValue(neighborPosition, out var existingConnection) ||
                        localConnection.Facing != direction ||
                        existingConnection.Facing != direction.Opposite())
                    {
                        candidate = default;
                        return false;
                    }

                    linkedExisting.Add(neighborPosition);
                    linkedCandidate.Add(worldPosition);
                }
            }
        }

        if (!linkedExisting.Contains(requiredConnection.Position))
        {
            candidate = default;
            return false;
        }

        if (!options.AllowLoops && linkedExisting.Count > 1)
        {
            candidate = default;
            return false;
        }

        foreach (var connection in variant.Connections)
        {
            var worldPosition = origin + connection.Position;
            if (linkedCandidate.Contains(worldPosition))
            {
                continue;
            }

            var outwardPosition = worldPosition + connection.Facing.Offset();
            if (state.OccupiedTiles.ContainsKey(outwardPosition))
            {
                candidate = default;
                return false;
            }
        }

        candidate = new CandidatePlacement(
            variant,
            origin,
            false,
            [.. linkedExisting],
            [.. linkedCandidate]);

        return true;
    }

    private static void AddPlacement(
        LayoutState state,
        PrefabVariant variant,
        Point2 origin,
        bool isCorridor,
        IReadOnlyCollection<Point2> linkedExistingConnectorPositions,
        IReadOnlyCollection<Point2> linkedCandidateConnectorPositions)
    {
        for (var y = 0; y < variant.Height; y++)
        {
            for (var x = 0; x < variant.Width; x++)
            {
                var tile = variant.Tiles[(y * variant.Width) + x];
                if (tile == TileKind.Empty)
                {
                    continue;
                }

                state.OccupiedTiles[origin + new Point2(x, y)] = tile;
            }
        }

        foreach (var existing in linkedExistingConnectorPositions)
        {
            state.OpenConnectors.Remove(existing);
            state.ConnectedConnectorPositions.Add(existing);
        }

        foreach (var candidateConnection in linkedCandidateConnectorPositions)
        {
            state.ConnectedConnectorPositions.Add(candidateConnection);
        }

        foreach (var connection in variant.Connections)
        {
            var worldPosition = origin + connection.Position;
            if (linkedCandidateConnectorPositions.Contains(worldPosition))
            {
                continue;
            }

            state.OpenConnectors[worldPosition] = new OpenConnector(worldPosition, connection.Facing);
        }

        state.Placements.Add(new Placement(variant, origin, isCorridor));
        if (isCorridor)
        {
            state.CorridorPlacementCount++;
        }
        else
        {
            state.RoomPlacementCount++;
        }
    }

    private static bool TryFinalize(LayoutState state, out GenerationResult result)
    {
        if (state.OccupiedTiles.Count == 0)
        {
            result = GenerationResult.Empty;
            return false;
        }

        var finalized = new Dictionary<Point2, TileKind>(state.OccupiedTiles.Count);
        foreach (var pair in state.OccupiedTiles)
        {
            finalized[pair.Key] = pair.Value switch
            {
                TileKind.Connector when state.ConnectedConnectorPositions.Contains(pair.Key) => TileKind.Floor,
                TileKind.Connector => TileKind.Wall,
                _ => pair.Value,
            };
        }

        if (!IsContiguous(finalized))
        {
            result = GenerationResult.Empty;
            return false;
        }

        var minX = finalized.Keys.Min(static point => point.X);
        var minY = finalized.Keys.Min(static point => point.Y);
        var maxX = finalized.Keys.Max(static point => point.X);
        var maxY = finalized.Keys.Max(static point => point.Y);
        var width = maxX - minX + 1;
        var height = maxY - minY + 1;
        var tiles = Enumerable.Repeat(TileKind.Empty, width * height).ToArray();

        foreach (var pair in finalized)
        {
            var x = pair.Key.X - minX;
            var y = pair.Key.Y - minY;
            tiles[(y * width) + x] = pair.Value;
        }

        var placedPrefabs = state.Placements
            .Select(placement => new PlacedPrefab(
                placement.Variant.Source.Name,
                new Point2(placement.Origin.X - minX, placement.Origin.Y - minY),
                placement.Variant.Transform,
                placement.Variant.Width,
                placement.Variant.Height,
                placement.IsCorridor))
            .ToArray();

        result = new GenerationResult(new LevelMap(tiles, width, height), placedPrefabs);
        return true;
    }

    private static bool IsContiguous(IReadOnlyDictionary<Point2, TileKind> tiles)
    {
        var walkable = tiles
            .Where(static pair => pair.Value == TileKind.Floor)
            .Select(static pair => pair.Key)
            .ToHashSet();

        if (walkable.Count == 0)
        {
            return false;
        }

        var visited = new HashSet<Point2>();
        var queue = new Queue<Point2>();
        var start = walkable.First();
        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var direction in AllDirections)
            {
                var next = current + direction.Offset();
                if (walkable.Contains(next) && visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return visited.Count == walkable.Count;
    }

    private static void ShuffleInPlace<T>(IList<T> items, Random random)
    {
        for (var index = items.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }
    }

    private sealed class LayoutState
    {
        public Dictionary<Point2, TileKind> OccupiedTiles { get; } = [];

        public Dictionary<Point2, OpenConnector> OpenConnectors { get; } = [];

        public HashSet<Point2> ConnectedConnectorPositions { get; } = [];

        public List<Placement> Placements { get; } = [];

        public int RoomPlacementCount { get; set; }

        public int CorridorPlacementCount { get; set; }

        public LayoutState Clone()
        {
            var clone = new LayoutState
            {
                RoomPlacementCount = RoomPlacementCount,
                CorridorPlacementCount = CorridorPlacementCount,
            };

            foreach (var pair in OccupiedTiles)
            {
                clone.OccupiedTiles.Add(pair.Key, pair.Value);
            }

            foreach (var pair in OpenConnectors)
            {
                clone.OpenConnectors.Add(pair.Key, pair.Value);
            }

            foreach (var position in ConnectedConnectorPositions)
            {
                clone.ConnectedConnectorPositions.Add(position);
            }

            clone.Placements.AddRange(Placements);
            return clone;
        }
    }

    private readonly record struct OpenConnector(Point2 Position, Direction Facing);

    private readonly record struct Placement(PrefabVariant Variant, Point2 Origin, bool IsCorridor);

    private readonly record struct CandidatePlacement(
        PrefabVariant Variant,
        Point2 Origin,
        bool IsCorridor,
        IReadOnlyCollection<Point2> LinkedExistingConnectorPositions,
        IReadOnlyCollection<Point2> LinkedCandidateConnectorPositions);
}
