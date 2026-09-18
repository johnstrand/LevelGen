namespace LevelGen;

public sealed class PrefabVariant
{
    public PrefabVariant(PrefabVariantOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Source = options.Source ?? throw new ArgumentNullException(nameof(options.Source));
        Transform = options.Transform;
        Width = options.Width;
        Height = options.Height;
        Tiles = options.Tiles?.ToArray() ?? throw new ArgumentNullException(nameof(options.Tiles));
        Connections = options.Connections?.ToArray() ?? throw new ArgumentNullException(nameof(options.Connections));
        LocalConnections = Connections.ToDictionary(connection => connection.Position);
        Doodads = options.Doodads?.ToArray() ?? throw new ArgumentNullException(nameof(options.Doodads));
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
