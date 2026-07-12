namespace LevelGen;

public sealed class PrefabDefinition
{
    private readonly TileKind[] _tiles;

    public PrefabDefinition(
        string name,
        int width,
        int height,
        IEnumerable<TileKind> tiles,
        IEnumerable<PrefabDoodad>? doodads = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Prefab name is required.", nameof(name));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        ArgumentNullException.ThrowIfNull(tiles);

        Name = name;
        Width = width;
        Height = height;
        _tiles = [.. tiles];
        if (_tiles.Length != width * height)
        {
            throw new ArgumentException("Tile count must match width * height.", nameof(tiles));
        }

        Doodads = [.. (doodads ?? [])];
    }

    public string Name { get; }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<PrefabDoodad> Doodads { get; }

    public TileKind this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            return _tiles[(y * Width) + x];
        }
    }

    public IReadOnlyList<TileKind> AsLinearTiles() => _tiles;
}
