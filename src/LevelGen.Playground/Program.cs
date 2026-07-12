using LevelGen;
using LevelGen.Blocks;
using LevelGen.Playground;

var settings = PlaygroundSettings.Parse(args);
var blocksPath = ResolveBlocksPath(settings.BlocksPath);
var prefabSet = BlocksPrefabParser.Parse(await File.ReadAllTextAsync(blocksPath));

Console.WriteLine($"Loaded {prefabSet.Count} prefabs from {blocksPath}");
if (!settings.RunOnce)
{
    Console.WriteLine("Press Enter to reroll, type a seed to reroll deterministically, or type q to quit.");
    Console.WriteLine();
}

while (true)
{
    var seed = settings.Seed ?? Random.Shared.Next();
    var result = LevelGenerator.Generate(
        prefabSet,
        new GenerationOptions
        {
            Seed = seed,
            MaxPrefabCount = settings.MaxPrefabCount,
            AllowGeneratedCorridors = settings.AllowGeneratedCorridors,
            AllowLoops = settings.AllowLoops,
            AllowMirrorTransforms = settings.AllowMirrorTransforms,
            MaxCorridorLength = settings.MaxCorridorLength,
        });

    Console.WriteLine($"Seed: {seed}");
    Console.WriteLine(
        $"Placements: {result.Placements.Count} ({result.Placements.Count(placement => !placement.IsCorridor)} rooms, {result.Placements.Count(placement => placement.IsCorridor)} corridors)");
    Console.WriteLine();
    WriteMap(result.Map);
    Console.WriteLine();
    WritePlacements(result.Placements);
    Console.WriteLine();

    if (settings.RunOnce)
    {
        break;
    }

    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();
    if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    settings = settings with
    {
        Seed = int.TryParse(input, out var nextSeed) ? nextSeed : null,
    };

    Console.WriteLine();
}

static void WriteMap(LevelMap map)
{
    for (var y = 0; y < map.Height; y++)
    {
        for (var x = 0; x < map.Width; x++)
        {
            Console.Write(
                map[x, y] switch
                {
                    TileKind.Empty => ' ',
                    TileKind.Wall => '#',
                    TileKind.Floor => '.',
                    TileKind.Connector => '*',
                    _ => '?',
                });
        }

        Console.WriteLine();
    }
}

static void WritePlacements(IReadOnlyList<PlacedPrefab> placements)
{
    Console.WriteLine("Placements:");
    foreach (var placement in placements)
    {
        Console.Write(" - ");
        Console.Write(placement.PrefabName);
        Console.Write(placement.IsCorridor ? " [corridor]" : " [room]");
        Console.Write($" at ({placement.Origin.X}, {placement.Origin.Y})");
        Console.Write($", size {placement.Width}x{placement.Height}");
        Console.Write($", rot {placement.Transform.QuarterTurnsClockwise * 90}deg");

        if (placement.Transform.MirrorHorizontally)
        {
            Console.Write(", mirrored");
        }

        Console.WriteLine();
    }
}

static string ResolveBlocksPath(string? explicitPath)
{
    if (!string.IsNullOrWhiteSpace(explicitPath))
    {
        var fullPath = Path.GetFullPath(explicitPath);

        return File.Exists(fullPath) ? fullPath : throw new FileNotFoundException("Could not find the requested blocks file.", fullPath);
    }

    foreach (var startDirectory in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
    {
        var directory = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "blocks.txt");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }
    }

    throw new FileNotFoundException("Could not locate blocks.txt. Pass a path with --blocks <path>.");
}
