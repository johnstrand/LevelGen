namespace LevelGen.Internal;

internal static class TileKindExtensions
{
    public static bool IsWalkable(this TileKind tileKind) =>
        tileKind is TileKind.Floor or TileKind.Connector;
}
