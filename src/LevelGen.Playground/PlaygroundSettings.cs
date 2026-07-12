namespace LevelGen.Playground;

internal sealed record PlaygroundSettings(
    int? Seed,
    string? BlocksPath,
    int MaxPrefabCount,
    bool AllowLoops,
    bool AllowGeneratedCorridors,
    bool AllowMirrorTransforms,
    int MaxCorridorLength,
    bool RunOnce)
{
    public static PlaygroundSettings Parse(string[] args)
    {
        int? seed = null;
        string? blocksPath = null;
        var maxPrefabCount = 6;
        var allowLoops = true;
        var allowGeneratedCorridors = true;
        var allowMirrorTransforms = true;
        var maxCorridorLength = 8;
        var runOnce = false;

        var index = 0;

        for (; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--seed":
                    seed = ParseInt(args, ++index, "--seed");
                    break;
                case "--blocks":
                    blocksPath = ParseString(args, ++index, "--blocks");
                    break;
                case "--max-prefabs":
                    maxPrefabCount = ParseInt(args, ++index, "--max-prefabs");
                    break;
                case "--max-corridor-length":
                    maxCorridorLength = ParseInt(args, ++index, "--max-corridor-length");
                    break;
                case "--no-loops":
                    allowLoops = false;
                    break;
                case "--no-corridors":
                    allowGeneratedCorridors = false;
                    break;
                case "--no-mirror":
                    allowMirrorTransforms = false;
                    break;
                case "--once":
                    runOnce = true;
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[index]}'. Use --help to see supported options.");
            }
        }

        return new PlaygroundSettings(
            seed,
            blocksPath,
            maxPrefabCount,
            allowLoops,
            allowGeneratedCorridors,
            allowMirrorTransforms,
            maxCorridorLength,
            runOnce);
    }

    private static int ParseInt(string[] args, int index, string optionName)
    {
        var raw = ParseString(args, index, optionName);
        return int.TryParse(raw, out var value)
            ? value
            : throw new ArgumentException($"Option {optionName} requires an integer value.");
    }

    private static string ParseString(string[] args, int index, string optionName)
    {
        return index >= args.Length ? throw new ArgumentException($"Option {optionName} requires a value.") : args[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("LevelGen playground");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --blocks <path>               Path to a blocks.txt file");
        Console.WriteLine("  --seed <int>                  Use a fixed seed");
        Console.WriteLine("  --max-prefabs <int>           Target number of room placements");
        Console.WriteLine("  --max-corridor-length <int>   Maximum generated corridor length");
        Console.WriteLine("  --no-loops                    Disallow extra loop connections");
        Console.WriteLine("  --no-corridors                Disable generated corridors");
        Console.WriteLine("  --no-mirror                   Disable mirrored prefab variants");
        Console.WriteLine("  --once                        Generate once and exit");
    }
}