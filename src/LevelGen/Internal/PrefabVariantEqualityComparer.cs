namespace LevelGen.Internal;

internal sealed class PrefabVariantEqualityComparer : IEqualityComparer<PrefabVariant>
{
    public static PrefabVariantEqualityComparer Instance { get; } = new();

    public bool Equals(PrefabVariant? x, PrefabVariant? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (x.Width != y.Width || x.Height != y.Height)
        {
            return false;
        }

        if (x.Tiles.Count != y.Tiles.Count || x.Connections.Count != y.Connections.Count)
        {
            return false;
        }

        for (var i = 0; i < x.Tiles.Count; i++)
        {
            ValidateTileKind(x.Tiles[i]);
            ValidateTileKind(y.Tiles[i]);

            if (x.Tiles[i] != y.Tiles[i])
            {
                return false;
            }
        }

        for (var i = 0; i < x.Connections.Count; i++)
        {
            if (x.Connections[i] != y.Connections[i])
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(PrefabVariant obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var hash = new HashCode();
        hash.Add(obj.Width);
        hash.Add(obj.Height);

        foreach (var tile in obj.Tiles)
        {
            ValidateTileKind(tile);
            hash.Add(tile);
        }

        foreach (var connection in obj.Connections)
        {
            hash.Add(connection);
        }

        return hash.ToHashCode();
    }

    private static void ValidateTileKind(TileKind tileKind)
    {
        switch (tileKind)
        {
            case TileKind.Empty:
            case TileKind.Wall:
            case TileKind.Floor:
            case TileKind.Connector:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tileKind));
        }
    }
}
