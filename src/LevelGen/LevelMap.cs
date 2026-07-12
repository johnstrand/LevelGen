namespace LevelGen;

public sealed class LevelMap
{
    private readonly TileKind[] _tiles;

    public LevelMap(IEnumerable<TileKind> tiles, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(tiles);

        ArgumentOutOfRangeException.ThrowIfNegative(width);

        ArgumentOutOfRangeException.ThrowIfNegative(height);

        _tiles = [.. tiles];
        if (_tiles.Length != width * height)
        {
            throw new ArgumentException("Tile count must match width * height.", nameof(tiles));
        }

        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public TileKind this[int x, int y]
    {
        get
        {
            return !Contains(x, y) ? throw new ArgumentOutOfRangeException($"({x}, {y})") : _tiles[(y * Width) + x];
        }
    }

    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public IReadOnlyList<TileKind> AsLinearTiles() => _tiles;
}
