namespace LevelGen.Tests;

public class LevelMapTests
{
    [Fact]
    public void Constructor_RejectsNullTiles()
    {
        Assert.Throws<ArgumentNullException>("tiles", () => new LevelMap(null!, 1, 1));
    }

    [Fact]
    public void Constructor_RejectsNegativeWidth()
    {
        Assert.Throws<ArgumentOutOfRangeException>("width", () => new LevelMap(Array.Empty<TileKind>(), -1, 1));
    }

    [Fact]
    public void Constructor_RejectsNegativeHeight()
    {
        Assert.Throws<ArgumentOutOfRangeException>("height", () => new LevelMap(Array.Empty<TileKind>(), 1, -1));
    }

    [Fact]
    public void Constructor_RejectsMismatchedTileCount()
    {
        Assert.Throws<ArgumentException>("tiles", () => new LevelMap(new[] { TileKind.Empty }, 2, 2));
    }
}
