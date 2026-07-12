using LevelGen.Blocks;
using LevelGen.Internal;

namespace LevelGen.Tests;

public sealed class ScaffoldTests
{
    [Fact]
    public void LevelGenerator_RejectsEmptyPrefabSets()
    {
        static GenerationResult Action() => LevelGenerator.Generate(new PrefabSet([]));

        var exception = Assert.Throws<ArgumentException>((Func<GenerationResult>)Action);
        Assert.Contains("At least one prefab is required", exception.Message);
    }

    [Fact]
    public void ParserFacade_RejectsBlankInput()
    {
        Assert.Throws<ArgumentException>(() => BlocksPrefabParser.Parse(" "));
    }

    [Fact]
    public void Parser_ReadsBlocksFilePrefabsAndDoodads()
    {
        var prefabSet = BlocksPrefabParser.Parse(LoadBlocksText());

        Assert.Equal(8, prefabSet.Count);

        var straightConnector = prefabSet.Single(prefab => prefab.Name == "Straight connector");
        Assert.Equal(5, straightConnector.Width);
        Assert.Equal(3, straightConnector.Height);

        var square = prefabSet.Single(prefab => prefab.Name == "Square");
        Assert.Equal(5, square.Doodads.Count);
        Assert.Contains(square.Doodads, doodad => doodad.Marker == '?');
    }

    [Fact]
    public void VariantFactory_DeduplicatesSymmetricTransforms()
    {
        var prefabSet = BlocksPrefabParser.Parse(LoadBlocksText());
        var straightConnector = prefabSet.Single(prefab => prefab.Name == "Straight connector");

        var variants = PrefabVariantFactory.CreateVariants(straightConnector, allowMirror: true);

        Assert.Equal(2, variants.Count);
        Assert.Contains(variants, variant => variant.Width == 5 && variant.Height == 3);
        Assert.Contains(variants, variant => variant.Width == 3 && variant.Height == 5);
    }

    [Fact]
    public void CorridorFactory_CreatesStraightAndElbowCorridors()
    {
        var corridors = CorridorPrefabFactory.CreateGeneratedCorridors(3);

        Assert.Equal(4, corridors.Count);
        Assert.Contains(corridors, corridor => corridor.Name == "Generated corridor elbow");
        Assert.Contains(corridors, corridor => corridor.Name == "Generated corridor (3)");
    }

    [Fact]
    public void Generator_IsDeterministicAndProducesContiguousFloorTiles()
    {
        var prefabSet = BlocksPrefabParser.Parse(LoadBlocksText());
        var options = new GenerationOptions
        {
            Seed = 12345,
            MaxPrefabCount = 4,
            AllowGeneratedCorridors = true,
            AllowLoops = true,
        };

        var first = LevelGenerator.Generate(prefabSet, options);
        var second = LevelGenerator.Generate(prefabSet, options);

        Assert.Equal(Render(first.Map), Render(second.Map));
        Assert.DoesNotContain(first.Map.AsLinearTiles(), tile => tile == TileKind.Connector);
        Assert.True(IsContiguous(first.Map));
        Assert.True(first.Placements.Count >= 1);
    }

    private static string Render(LevelMap map)
    {
        var rows = new string[map.Height];
        for (var y = 0; y < map.Height; y++)
        {
            var row = new char[map.Width];
            for (var x = 0; x < map.Width; x++)
            {
                row[x] = map[x, y] switch
                {
                    TileKind.Empty => ' ',
                    TileKind.Wall => '#',
                    TileKind.Floor => '.',
                    TileKind.Connector => '*',
                    _ => '?',
                };
            }

            rows[y] = new string(row);
        }

        return string.Join(Environment.NewLine, rows);
    }

    private static bool IsContiguous(LevelMap map)
    {
        var walkable = new HashSet<Point2>();
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (map[x, y] == TileKind.Floor)
                {
                    walkable.Add(new Point2(x, y));
                }
            }
        }

        if (walkable.Count == 0)
        {
            return false;
        }

        var start = walkable.First();
        var queue = new Queue<Point2>();
        var visited = new HashSet<Point2> { start };
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var direction in Enum.GetValues<Direction>())
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

    private static string LoadBlocksText() => File.ReadAllText(ResolveBlocksPath());

    private static string ResolveBlocksPath()
    {
        foreach (var startDirectory in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(Path.GetFullPath(startDirectory));
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "src", "LevelGen.Playground", "blocks.txt");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException("Could not locate src\\LevelGen.Playground\\blocks.txt.");
    }
}
