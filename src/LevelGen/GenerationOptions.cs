namespace LevelGen;

public sealed class GenerationOptions
{
    public int Seed { get; init; }

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
