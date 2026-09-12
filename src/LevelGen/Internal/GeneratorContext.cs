namespace LevelGen.Internal;

internal sealed record GeneratorContext(
    IReadOnlyList<PrefabVariant> RoomVariants,
    IReadOnlyList<PrefabVariant> CorridorVariants,
    int TargetRoomPlacements,
    GenerationOptions Options,
    Random Random)
{
    public HashSet<Point2> ScratchLinkedExisting { get; } = [];
    public HashSet<Point2> ScratchLinkedCandidate { get; } = [];
}
