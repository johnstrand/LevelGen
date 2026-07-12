using LevelGen.Internal;

namespace LevelGen;

public static class LevelGenerator
{
    public static GenerationResult Generate(PrefabSet prefabSet, GenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(prefabSet);

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0046 // Convert to conditional expression
        if (prefabSet.Count == 0)
        {
            throw new ArgumentException("At least one prefab is required to generate a level.", nameof(prefabSet));
        }
#pragma warning restore IDE0046 // Convert to conditional expression
#pragma warning restore IDE0079 // Remove unnecessary suppression

        var resolvedOptions = options ?? new GenerationOptions();
        ValidateOptions(resolvedOptions);

        return GeneratorCore.Generate(prefabSet, resolvedOptions);
    }

    private static void ValidateOptions(GenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxPrefabCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxPrefabCount), "MaxPrefabCount must be greater than 0 when specified.");
        }

        if (options.TargetWalkableTileCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.TargetWalkableTileCount),
                "TargetWalkableTileCount must be greater than 0 when specified.");
        }

        if (options.MaxCorridorLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxCorridorLength), "MaxCorridorLength must be greater than 0.");
        }
    }
}
