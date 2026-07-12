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

        return GeneratorCore.Generate(prefabSet, options ?? new GenerationOptions());
    }
}
