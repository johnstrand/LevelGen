namespace LevelGen.Internal;

internal static class CorridorPrefabFactory
{
    public static IReadOnlyList<PrefabDefinition> CreateGeneratedCorridors(int maxCorridorLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxCorridorLength, 1);

        var corridors = new List<PrefabDefinition>();
        for (var floorLength = 1; floorLength <= maxCorridorLength; floorLength++)
        {
            corridors.Add(CreateStraightCorridor(floorLength));
        }

        corridors.Add(CreateElbowCorridor());
        return corridors;
    }

    public static PrefabDefinition CreateStraightCorridor(int floorLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(floorLength, 1);

        var corridorRow = $"*{new string('.', floorLength)}*";
        var wallRow = new string('#', corridorRow.Length);

        return ParsePrefab(
            $"Generated corridor ({floorLength})",
            wallRow,
            corridorRow,
            wallRow);
    }

    public static PrefabDefinition CreateElbowCorridor() =>
        ParsePrefab(
            "Generated corridor elbow",
            "##*#",
            "#..#",
            "*..#",
            "####");

    private static PrefabDefinition ParsePrefab(string name, params string[] rows)
    {
        var width = rows.Max(static row => row.Length);
        var tiles = new List<TileKind>(width * rows.Length);

        foreach (var row in rows)
        {
            var padded = row.PadRight(width, ' ');
            foreach (var token in padded)
            {
                tiles.Add(token switch
                {
                    '#' => TileKind.Wall,
                    '.' => TileKind.Floor,
                    '*' => TileKind.Connector,
                    ' ' => TileKind.Empty,
                    _ => throw new InvalidOperationException($"Unsupported generated corridor token '{token}'."),
                });
            }
        }

        return new PrefabDefinition(name, width, rows.Length, tiles);
    }
}
