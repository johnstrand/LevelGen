using LevelGen.Blocks;

namespace LevelGen.Internal;

internal static class BlocksPrefabParserCore
{
    public static PrefabSet Parse(string text)
    {
        var prefabs = new List<PrefabDefinition>();
        var rows = new List<string>();
        string? currentName = null;

        foreach (var rawLine in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var trimmedStart = line.TrimStart();

            if (trimmedStart.StartsWith('/'))
            {
                continue;
            }

            if (trimmedStart.StartsWith('>'))
            {
                FinalizePrefab(prefabs, rows, ref currentName);

                currentName = trimmedStart[1..].Trim();
                if (string.IsNullOrWhiteSpace(currentName))
                {
                    throw new FormatException("Prefab names cannot be blank.");
                }

                continue;
            }

            if (trimmedStart.Length == 0)
            {
                FinalizePrefab(prefabs, rows, ref currentName);
                continue;
            }

            if (currentName is null)
            {
                throw new FormatException($"Encountered prefab tiles before a section header: '{line}'.");
            }

            rows.Add(line);
        }

        FinalizePrefab(prefabs, rows, ref currentName);

        if (prefabs.Count == 0)
        {
            throw new FormatException($"No prefabs were found in the supplied {nameof(BlocksPrefabParser)} input.");
        }

        return new PrefabSet(prefabs);
    }

    private static void FinalizePrefab(List<PrefabDefinition> prefabs, List<string> rows, ref string? currentName)
    {
        if (currentName is null)
        {
            rows.Clear();
            return;
        }

        if (rows.Count == 0)
        {
            currentName = null;
            return;
        }

        var width = rows.Max(static row => row.Length);
        var tiles = new List<TileKind>(width * rows.Count);
        var doodads = new List<PrefabDoodad>();

        for (var y = 0; y < rows.Count; y++)
        {
            var paddedRow = rows[y].PadRight(width, ' ');
            for (var x = 0; x < paddedRow.Length; x++)
            {
                var token = paddedRow[x];
                switch (token)
                {
                    case '#':
                        tiles.Add(TileKind.Wall);
                        break;
                    case '.':
                        tiles.Add(TileKind.Floor);
                        break;
                    case '*':
                        tiles.Add(TileKind.Connector);
                        break;
                    case '?':
                        tiles.Add(TileKind.Floor);
                        doodads.Add(new PrefabDoodad(new Point2(x, y), '?'));
                        break;
                    case >= 'A' and <= 'P':
                        tiles.Add(TileKind.Floor);
                        doodads.Add(new PrefabDoodad(new Point2(x, y), token));
                        break;
                    case ' ':
                        tiles.Add(TileKind.Empty);
                        break;
                    default:
                        throw new FormatException($"Unsupported tile token '{token}' in prefab '{currentName}'.");
                }
            }
        }

        prefabs.Add(new PrefabDefinition(currentName, width, rows.Count, tiles, doodads));
        rows.Clear();
        currentName = null;
    }
}
