namespace LevelGen;

public sealed record PlacedPrefab(
    string PrefabName,
    Point2 Origin,
    PrefabTransform Transform,
    int Width,
    int Height,
    bool IsCorridor = false);
