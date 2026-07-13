namespace LevelGen;

public sealed class PrefabVariant(
    PrefabDefinition source,
    PrefabTransform transform,
    int width,
    int height,
    IEnumerable<TileKind> tiles,
    IEnumerable<PrefabConnectionPoint> connections,
    IEnumerable<PrefabDoodad> doodads)
{
    public PrefabDefinition Source { get; } = source ?? throw new ArgumentNullException(nameof(source));

    public PrefabTransform Transform { get; } = transform;

    public int Width { get; } = width;

    public int Height { get; } = height;

    public IReadOnlyList<TileKind> Tiles { get; } = tiles?.ToArray() ?? throw new ArgumentNullException(nameof(tiles));

    public IReadOnlyList<PrefabConnectionPoint> Connections { get; } = connections?.ToArray() ?? throw new ArgumentNullException(nameof(connections));

    internal IReadOnlyDictionary<Point2, PrefabConnectionPoint> LocalConnections { get; } = connections?.ToDictionary(connection => connection.Position) ?? throw new ArgumentNullException(nameof(connections));

    public IReadOnlyList<PrefabDoodad> Doodads { get; } = doodads?.ToArray() ?? throw new ArgumentNullException(nameof(doodads));
}
