namespace LevelGen.Internal;

internal static class PrefabVariantFactory
{
    public static IReadOnlyList<PrefabConnectionPoint> ExtractConnections(PrefabDefinition prefab)
    {
        ArgumentNullException.ThrowIfNull(prefab);

        var result = new List<PrefabConnectionPoint>();
        for (var y = 0; y < prefab.Height; y++)
        {
            for (var x = 0; x < prefab.Width; x++)
            {
                if (prefab[x, y] != TileKind.Connector)
                {
                    continue;
                }

                if (TryInferConnectorFacing(prefab, x, y, out var facing))
                {
                    result.Add(new PrefabConnectionPoint(new Point2(x, y), facing));
                }
            }
        }

        return result;
    }

    public static IReadOnlyList<PrefabVariant> CreateVariants(PrefabDefinition prefab, bool allowMirror)
    {
        ArgumentNullException.ThrowIfNull(prefab);

        var connections = ExtractConnections(prefab);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var variants = new List<PrefabVariant>();
        var mirrorStates = allowMirror ? new[] { false, true } : [false];

        foreach (var mirror in mirrorStates)
        {
            for (var quarterTurns = 0; quarterTurns < 4; quarterTurns++)
            {
                var transform = new PrefabTransform(quarterTurns, mirror);
                var (width, height) = GetTransformedSize(prefab.Width, prefab.Height, transform);
                var tiles = Enumerable.Repeat(TileKind.Empty, width * height).ToArray();

                for (var y = 0; y < prefab.Height; y++)
                {
                    for (var x = 0; x < prefab.Width; x++)
                    {
                        var tile = prefab[x, y];
                        if (tile == TileKind.Connector && !connections.Any(connection => connection.Position == new Point2(x, y)))
                        {
                            tile = TileKind.Floor;
                        }

                        var transformed = TransformPoint(new Point2(x, y), prefab.Width, prefab.Height, transform);
                        tiles[(transformed.Y * width) + transformed.X] = tile;
                    }
                }

                var transformedConnections = connections
                    .Select(connection => new PrefabConnectionPoint(
                        TransformPoint(connection.Position, prefab.Width, prefab.Height, transform),
                        TransformDirection(connection.Facing, transform)))
                    .OrderBy(connection => connection.Position.Y)
                    .ThenBy(connection => connection.Position.X)
                    .ThenBy(connection => connection.Facing)
                    .ToArray();

                var transformedDoodads = prefab.Doodads
                    .Select(doodad => new PrefabDoodad(
                        TransformPoint(doodad.Position, prefab.Width, prefab.Height, transform),
                        doodad.Marker))
                    .OrderBy(doodad => doodad.Position.Y)
                    .ThenBy(doodad => doodad.Position.X)
                    .ThenBy(doodad => doodad.Marker)
                    .ToArray();

                var variant = new PrefabVariant(
                    prefab,
                    transform,
                    width,
                    height,
                    tiles,
                    transformedConnections,
                    transformedDoodads);

                if (seen.Add(CreateVariantKey(variant)))
                {
                    variants.Add(variant);
                }
            }
        }

        return variants;
    }

    public static bool TryInferConnectorFacing(PrefabDefinition prefab, int x, int y, out Direction facing)
    {
        var outwardCandidates = Enum.GetValues<Direction>()
            .Where(direction => IsOutward(prefab, x, y, direction))
            .ToArray();

        if (outwardCandidates.Length == 0)
        {
            facing = default;
            return false;
        }

        if (outwardCandidates.Length == 1)
        {
            facing = outwardCandidates[0];
            return true;
        }

        var inwardCandidates = outwardCandidates
            .Where(direction =>
            {
                var opposite = direction.Opposite().Offset();
                var oppositeX = x + opposite.X;
                var oppositeY = y + opposite.Y;
                return oppositeX >= 0 &&
                    oppositeX < prefab.Width &&
                    oppositeY >= 0 &&
                    oppositeY < prefab.Height &&
                    prefab[oppositeX, oppositeY].IsWalkable();
            })
            .ToArray();

        if (inwardCandidates.Length == 1)
        {
            facing = inwardCandidates[0];
            return true;
        }

        throw new InvalidOperationException($"Connector at ({x}, {y}) in prefab '{prefab.Name}' must expose exactly one outward-facing side.");
    }

    private static bool IsOutward(PrefabDefinition prefab, int x, int y, Direction direction)
    {
        var offset = direction.Offset();
        var neighborX = x + offset.X;
        var neighborY = y + offset.Y;

        return neighborX < 0 ||
            neighborX >= prefab.Width ||
            neighborY < 0 ||
            neighborY >= prefab.Height ||
            prefab[neighborX, neighborY] == TileKind.Empty;
    }

    private static string CreateVariantKey(PrefabVariant variant)
    {
        var tileKey = new string([.. variant.Tiles.Select(ToToken)]);
        var connectionKey = string.Join(
            ";",
            variant.Connections.Select(connection =>
                $"{connection.Position.X},{connection.Position.Y},{(int)connection.Facing}"));

        return $"{variant.Width}x{variant.Height}|{tileKey}|{connectionKey}";
    }

    private static char ToToken(TileKind tileKind) =>
        tileKind switch
        {
            TileKind.Empty => ' ',
            TileKind.Wall => '#',
            TileKind.Floor => '.',
            TileKind.Connector => '*',
            _ => throw new ArgumentOutOfRangeException(nameof(tileKind)),
        };

    public static (int Width, int Height) GetTransformedSize(int width, int height, PrefabTransform transform) =>
        transform.QuarterTurnsClockwise % 2 == 0 ? (width, height) : (height, width);

    public static Point2 TransformPoint(Point2 point, int width, int height, PrefabTransform transform)
    {
        var x = point.X;
        var y = point.Y;
        var currentWidth = width;
        var currentHeight = height;

        if (transform.MirrorHorizontally)
        {
            x = currentWidth - 1 - x;
        }

        for (var i = 0; i < transform.QuarterTurnsClockwise; i++)
        {
            (x, y) = (currentHeight - 1 - y, x);
            (currentWidth, currentHeight) = (currentHeight, currentWidth);
        }

        return new Point2(x, y);
    }

    public static Direction TransformDirection(Direction direction, PrefabTransform transform)
    {
        var transformed = transform.MirrorHorizontally
            ? direction.MirrorHorizontally()
            : direction;

        return transformed.RotateClockwise(transform.QuarterTurnsClockwise);
    }
}
