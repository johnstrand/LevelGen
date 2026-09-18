namespace LevelGen;

public sealed record PrefabVariantOptions
{
    public PrefabDefinition Source { get; init; } = null!;

    public PrefabTransform Transform { get; init; } = PrefabTransform.Identity;

    public int Width { get; init; }

    public int Height { get; init; }

    public IEnumerable<TileKind> Tiles { get; init; } = [];

    public IEnumerable<PrefabConnectionPoint> Connections { get; init; } = [];

    public IEnumerable<PrefabDoodad> Doodads { get; init; } = [];
}

public sealed class PrefabVariantBuilder
{
    private PrefabDefinition? _source;
    private PrefabTransform _transform = PrefabTransform.Identity;
    private int _width;
    private int _height;
    private IEnumerable<TileKind>? _tiles;
    private IEnumerable<PrefabConnectionPoint>? _connections;
    private IEnumerable<PrefabDoodad>? _doodads;

    public PrefabVariantBuilder WithSource(PrefabDefinition source)
    {
        _source = source;
        return this;
    }

    public PrefabVariantBuilder WithTransform(PrefabTransform transform)
    {
        _transform = transform;
        return this;
    }

    public PrefabVariantBuilder WithWidth(int width)
    {
        _width = width;
        return this;
    }

    public PrefabVariantBuilder WithHeight(int height)
    {
        _height = height;
        return this;
    }

    public PrefabVariantBuilder WithDimensions(int width, int height)
    {
        _width = width;
        _height = height;
        return this;
    }

    public PrefabVariantBuilder WithTiles(IEnumerable<TileKind> tiles)
    {
        _tiles = tiles;
        return this;
    }

    public PrefabVariantBuilder WithConnections(IEnumerable<PrefabConnectionPoint> connections)
    {
        _connections = connections;
        return this;
    }

    public PrefabVariantBuilder WithDoodads(IEnumerable<PrefabDoodad> doodads)
    {
        _doodads = doodads;
        return this;
    }

    public PrefabVariantOptions BuildOptions()
    {
        return new PrefabVariantOptions
        {
            Source = _source!,
            Transform = _transform,
            Width = _width,
            Height = _height,
            Tiles = _tiles!,
            Connections = _connections!,
            Doodads = _doodads!,
        };
    }

    public PrefabVariant Build()
    {
        return new PrefabVariant(BuildOptions());
    }
}
