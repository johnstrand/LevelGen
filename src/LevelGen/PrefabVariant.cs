namespace LevelGen;

public sealed class PrefabVariant
{
    public PrefabVariant(
        PrefabDefinition source,
        PrefabTransform transform,
        int width,
        int height,
        IEnumerable<TileKind> tiles,
        IEnumerable<PrefabConnectionPoint> connections,
        IEnumerable<PrefabDoodad> doodads)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Transform = transform;
        Width = width;
        Height = height;
        Tiles = tiles?.ToArray() ?? throw new ArgumentNullException(nameof(tiles));
        Connections = connections?.ToArray() ?? throw new ArgumentNullException(nameof(connections));
        LocalConnections = Connections.ToDictionary(connection => connection.Position);
        Doodads = doodads?.ToArray() ?? throw new ArgumentNullException(nameof(doodads));
    }

    public PrefabDefinition Source { get; }

    public PrefabTransform Transform { get; }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<TileKind> Tiles { get; }

    public IReadOnlyList<PrefabConnectionPoint> Connections { get; }

    internal IReadOnlyDictionary<Point2, PrefabConnectionPoint> LocalConnections { get; }

    public IReadOnlyList<PrefabDoodad> Doodads { get; }
}
