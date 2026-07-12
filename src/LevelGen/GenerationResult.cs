namespace LevelGen;

public sealed class GenerationResult(LevelMap map, IReadOnlyList<PlacedPrefab> placements)
{
    public LevelMap Map { get; } = map ?? throw new ArgumentNullException(nameof(map));

    public IReadOnlyList<PlacedPrefab> Placements { get; } = placements ?? throw new ArgumentNullException(nameof(placements));

    public static GenerationResult Empty { get; } = new(new LevelMap([], 0, 0), []);
}
