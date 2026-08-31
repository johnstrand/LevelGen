using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LevelGen.Internal;

internal static class GeneratorCore
{

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
        var context = new GeneratorContext(roomVariants, corridorVariants, targetRoomPlacements, options, random);

        GenerationResult? bestResult = null;
        int minDeviation = int.MaxValue;

        for (var attempt = 0; attempt < 12; attempt++)
        {
            var seed = context.RoomVariants[context.Random.Next(context.RoomVariants.Count)];
            var state = new LayoutState();
            AddPlacement(state, seed, new Point2(0, 0), isCorridor: false, [], []);

            if (TryExpand(context, state, depth: 0, out var result))
            {
                var deviation = CalculateDeviation(result.Map.Width, result.Map.Height, options);
                if (deviation == 0)
                {
                    return result;
                }

                if (deviation < minDeviation)
                {
                    minDeviation = deviation;
                    bestResult = result;
                }
            }
        }

        if (bestResult != null)
        {
            return bestResult;
        }

        throw new InvalidOperationException("Unable to generate a valid contiguous level from the supplied prefabs.");
    }

    private static int CalculateDeviation(int width, int height, GenerationOptions options)
    {
        int dev = 0;
        if (options.MinWidth.HasValue && width < options.MinWidth.Value)
        {
            dev += options.MinWidth.Value - width;
        }

        if (options.MaxWidth.HasValue && width > options.MaxWidth.Value)
        {
            dev += width - options.MaxWidth.Value;
        }

        if (options.MinHeight.HasValue && height < options.MinHeight.Value)
        {
            dev += options.MinHeight.Value - height;
        }

        if (options.MaxHeight.HasValue && height > options.MaxHeight.Value)
        {
            dev += height - options.MaxHeight.Value;
        }

        return dev;
    }

    private static bool TryExpand(
        GeneratorContext context,
        LayoutState state,
        int depth,
        out GenerationResult result)
    {
        if (depth > 128)
        {
            result = GenerationResult.Empty;
            return false;
        }

        if ((state.RoomPlacementCount >= context.TargetRoomPlacements && TryFinalize(state, out result)) ||
            (state.OpenConnectors.Count == 0 && TryFinalize(state, out result)))
        {
            return true;
        }

        if (context.Options.MaxWidth.HasValue || context.Options.MaxHeight.HasValue)
        {
            var (minX, minY, currentW, currentH) = CalculateStateBounds(state);
            if ((context.Options.MaxWidth.HasValue && currentW > context.Options.MaxWidth.Value) ||
                (context.Options.MaxHeight.HasValue && currentH > context.Options.MaxHeight.Value))
            {
                result = GenerationResult.Empty;
                return false;
            }
        }

        if (!ChooseNextConnector(context, state, out var selectedConnector, out var roomCandidates))
        {
            result = GenerationResult.Empty;
            return TryFinalize(state, out result);
        }

        var orderedCandidates = GetShuffledCandidates(
            context, state, selectedConnector, roomCandidates);

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

            if (TryExpand(context, nextState, depth + 1, out result))
            {
                return true;
            }
        }

        if (state.OpenConnectors.Remove(selectedConnector.Position) &&
            TryExpand(context, state, depth + 1, out result))
        {
            return true;
        }

        state.OpenConnectors[selectedConnector.Position] = selectedConnector;
        result = GenerationResult.Empty;
        return false;
    }

    private static IList<CandidatePlacement> GetShuffledCandidates(
        GeneratorContext context,
        LayoutState state,
        OpenConnector selectedConnector,
        List<CandidatePlacement> roomCandidates)
    {
        var orderedCandidates = new List<CandidatePlacement>(roomCandidates);

        var roomCandidateCount = roomCandidates.Count;
        var canUseCorridors =
            context.Options.AllowGeneratedCorridors &&
            context.CorridorVariants.Count > 0 &&
            state.CorridorPlacementCount < Math.Max(1, context.TargetRoomPlacements * 2) &&
            (roomCandidateCount == 0 || context.Random.NextDouble() < 0.35);

        if (canUseCorridors)
        {
            var corridorBuffer = new List<CandidatePlacement>();
            BuildCandidates(context, state, selectedConnector, context.CorridorVariants, isCorridor: true, corridorBuffer);
            orderedCandidates.AddRange(corridorBuffer);
        }

        ShuffleInPlace(orderedCandidates, context.Random);

        return orderedCandidates;
    }

    private static bool ChooseNextConnector(
        GeneratorContext context,
        LayoutState state,
        out OpenConnector bestConnector,
        [NotNullWhen(true)] out List<CandidatePlacement>? bestRoomCandidates)
    {
        bool found = false;
        bestConnector = default;
        bestRoomCandidates = null;
        var bestScore = int.MaxValue;
        var roomBuffer = new List<CandidatePlacement>();
        var corridorBuffer = new List<CandidatePlacement>();

        foreach (var connector in state.OpenConnectors.Values)
        {
            BuildCandidates(context, state, connector, context.RoomVariants, isCorridor: false, roomBuffer);
            var score = roomBuffer.Count;
            if (score == 0 && context.Options.AllowGeneratedCorridors)
            {
                BuildCandidates(context, state, connector, context.CorridorVariants, isCorridor: false, corridorBuffer);
                score = corridorBuffer.Count;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestConnector = connector;
                bestRoomCandidates = new List<CandidatePlacement>(roomBuffer);
                found = true;
            }
        }

        return found;
    }

    private static void BuildCandidates(
        GeneratorContext context,
        LayoutState state,
        OpenConnector openConnector,
        IReadOnlyList<PrefabVariant> variants,
        bool isCorridor,
        List<CandidatePlacement> targetBuffer)
    {
        targetBuffer.Clear();
        foreach (var variant in variants)
        {
            foreach (var connection in variant.Connections)
            {
                if (connection.Facing != openConnector.Facing.Opposite())
                {
                    continue;
                }

                var origin = openConnector.Position + openConnector.Facing.Offset() - connection.Position;
                if (TryValidatePlacement(context, state, variant, origin, openConnector, out var candidate))
                {
                    targetBuffer.Add(candidate with { IsCorridor = isCorridor });
                }
            }
        }

        if ((context.Options.MaxWidth.HasValue || context.Options.MaxHeight.HasValue) && targetBuffer.Count > 1)
        {
            var anyFits = false;
            for (int i = 0; i < targetBuffer.Count; i++)
            {
                var candidate = targetBuffer[i];
                if (PlacementFitsMaxBounds(state, candidate.Variant, candidate.Origin, context.Options.MaxWidth, context.Options.MaxHeight))
                {
                    anyFits = true;
                    break;
                }
            }

            if (anyFits)
            {
                targetBuffer.RemoveAll(candidate => !PlacementFitsMaxBounds(state, candidate.Variant, candidate.Origin, context.Options.MaxWidth, context.Options.MaxHeight));
            }
        }
    }

    private static bool PlacementFitsMaxBounds(
        LayoutState state,
        PrefabVariant variant,
        Point2 origin,
        int? maxWidth,
        int? maxHeight)
    {
        var minX = state.MinX;
        var minY = state.MinY;
        var maxX = state.MaxX;
        var maxY = state.MaxY;

        for (var y = 0; y < variant.Height; y++)
        {
            for (var x = 0; x < variant.Width; x++)
            {
                var tile = variant.Tiles[(y * variant.Width) + x];
                if (tile == TileKind.Empty)
                {
                    continue;
                }

                var pt = origin + new Point2(x, y);
                if (pt.X < minX) minX = pt.X;
                if (pt.Y < minY) minY = pt.Y;
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y > maxY) maxY = pt.Y;
            }
        }

        if (maxWidth.HasValue && (maxX - minX + 1) > maxWidth.Value)
        {
            return false;
        }

        if (maxHeight.HasValue && (maxY - minY + 1) > maxHeight.Value)
        {
            return false;
        }

        return true;
    }

    private static bool TryValidatePlacement(
        GeneratorContext context,
        LayoutState state,
        PrefabVariant variant,
        Point2 origin,
        OpenConnector requiredConnection,
        out CandidatePlacement candidate)
    {
        var linkedExisting = new HashSet<Point2>();
        var linkedCandidate = new HashSet<Point2>();

        if (!TryValidateTilesAndConnections(state, variant, origin, linkedExisting, linkedCandidate))
        {
            candidate = default;
            return false;
        }

        if (!linkedExisting.Contains(requiredConnection.Position))
        {
            candidate = default;
            return false;
        }

        if (!context.Options.AllowLoops && linkedExisting.Count > 1)
        {
            candidate = default;
            return false;
        }

        if (!TryValidateOutwardConnections(state, variant, origin, linkedCandidate))
        {
            candidate = default;
            return false;
        }

        candidate = new CandidatePlacement(
            variant,
            origin,
            false,
            linkedExisting,
            linkedCandidate);

        return true;
    }

    private static bool TryValidateTilesAndConnections(
        LayoutState state,
        PrefabVariant variant,
        Point2 origin,
        HashSet<Point2> linkedExisting,
        HashSet<Point2> linkedCandidate)
    {
        var localConnections = variant.LocalConnections;

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
                    return false;
                }

                foreach (var direction in DirectionExtensions.AllDirections)
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
                        return false;
                    }

                    linkedExisting.Add(neighborPosition);
                    linkedCandidate.Add(worldPosition);
                }
            }
        }

        return true;
    }

    private static bool TryValidateOutwardConnections(
        LayoutState state,
        PrefabVariant variant,
        Point2 origin,
        HashSet<Point2> linkedCandidate)
    {
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
                return false;
            }
        }

        return true;
    }

    private static void AddPlacement(
        LayoutState state,
        PrefabVariant variant,
        Point2 origin,
        bool isCorridor,
        HashSet<Point2> linkedExistingConnectorPositions,
        HashSet<Point2> linkedCandidateConnectorPositions)
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

                var worldPos = origin + new Point2(x, y);
                state.OccupiedTiles[worldPos] = tile;
                if (worldPos.X < state.MinX) state.MinX = worldPos.X;
                if (worldPos.Y < state.MinY) state.MinY = worldPos.Y;
                if (worldPos.X > state.MaxX) state.MaxX = worldPos.X;
                if (worldPos.Y > state.MaxY) state.MaxY = worldPos.Y;
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

        var finalized = BuildFinalizedTiles(state);

        if (!IsContiguous(finalized))
        {
            result = GenerationResult.Empty;
            return false;
        }

        var bounds = CalculateBounds(finalized);
        result = BuildGenerationResult(state, finalized, bounds);
        return true;
    }

    private static Dictionary<Point2, TileKind> BuildFinalizedTiles(LayoutState state)
    {
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

        return finalized;
    }

    private static (int MinX, int MinY, int Width, int Height) CalculateStateBounds(LayoutState state)
    {
        if (state.OccupiedTiles.Count == 0)
        {
            return (0, 0, 0, 0);
        }

        var width = state.MaxX - state.MinX + 1;
        var height = state.MaxY - state.MinY + 1;
        return (state.MinX, state.MinY, width, height);
    }

    private static (int MinX, int MinY, int Width, int Height) CalculateBounds(Dictionary<Point2, TileKind> finalized)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        foreach (var point in finalized.Keys)
        {
            if (point.X < minX) minX = point.X;
            if (point.Y < minY) minY = point.Y;
            if (point.X > maxX) maxX = point.X;
            if (point.Y > maxY) maxY = point.Y;
        }

        var width = maxX - minX + 1;
        var height = maxY - minY + 1;
        return (minX, minY, width, height);
    }

    private static GenerationResult BuildGenerationResult(
        LayoutState state,
        Dictionary<Point2, TileKind> finalized,
        (int MinX, int MinY, int Width, int Height) bounds)
    {
        var (minX, minY, width, height) = bounds;
        var tiles = new TileKind[width * height];

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

        return new GenerationResult(new LevelMap(tiles, width, height), placedPrefabs);
    }

    private static bool IsContiguous(IReadOnlyDictionary<Point2, TileKind> tiles)
    {
        var walkable = new HashSet<Point2>();
        foreach (var pair in tiles)
        {
            if (pair.Value == TileKind.Floor)
            {
                walkable.Add(pair.Key);
            }
        }

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
            foreach (var direction in DirectionExtensions.AllDirections)
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

    internal sealed class LayoutState
    {
        public Dictionary<Point2, TileKind> OccupiedTiles { get; }

        public Dictionary<Point2, OpenConnector> OpenConnectors { get; }

        public HashSet<Point2> ConnectedConnectorPositions { get; }

        public List<Placement> Placements { get; }

        public int RoomPlacementCount { get; set; }

        public int CorridorPlacementCount { get; set; }

        public int MinX { get; set; } = int.MaxValue;

        public int MinY { get; set; } = int.MaxValue;

        public int MaxX { get; set; } = int.MinValue;

        public int MaxY { get; set; } = int.MinValue;

        public LayoutState()
        {
            OccupiedTiles = [];
            OpenConnectors = [];
            ConnectedConnectorPositions = [];
            Placements = [];
        }

        private LayoutState(LayoutState other)
        {
            OccupiedTiles = new(other.OccupiedTiles);
            OpenConnectors = new(other.OpenConnectors);
            ConnectedConnectorPositions = new(other.ConnectedConnectorPositions);
            Placements = new(other.Placements);
            RoomPlacementCount = other.RoomPlacementCount;
            CorridorPlacementCount = other.CorridorPlacementCount;
            MinX = other.MinX;
            MinY = other.MinY;
            MaxX = other.MaxX;
            MaxY = other.MaxY;
        }

        public LayoutState Clone()
        {
            return new LayoutState(this);
        }
    }

    internal readonly record struct OpenConnector(Point2 Position, Direction Facing);

    internal readonly record struct Placement(PrefabVariant Variant, Point2 Origin, bool IsCorridor);

    private readonly record struct CandidatePlacement(
        PrefabVariant Variant,
        Point2 Origin,
        bool IsCorridor,
        HashSet<Point2> LinkedExistingConnectorPositions,
        HashSet<Point2> LinkedCandidateConnectorPositions);
}
