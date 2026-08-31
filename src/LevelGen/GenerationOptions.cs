namespace LevelGen;

public sealed class GenerationOptions
{
    public int Seed { get; init; }

    /// <summary>
    /// Gets the desired number of walkable tiles in the generated level.
    /// </summary>
    /// <remarks>
    /// This option is currently accepted and validated but not yet used by the generation
    /// engine. Setting it has no effect on the output.
    /// </remarks>
    public int? TargetWalkableTileCount { get; init; }

    public int? MaxPrefabCount { get; init; } = 6;

    public int? MinWidth { get; init; }

    public int? MaxWidth { get; init; }

    public int? MinHeight { get; init; }

    public int? MaxHeight { get; init; }

    public bool AllowLoops { get; init; } = true;

    public bool AllowMirrorTransforms { get; init; } = true;

    public bool AllowGeneratedCorridors { get; init; } = true;

    public int MaxCorridorLength { get; init; } = 8;
}
