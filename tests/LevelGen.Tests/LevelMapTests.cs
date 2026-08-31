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

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, true)]
    [InlineData(0, 2, true)]
    [InlineData(2, 2, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(3, 0, false)]
    [InlineData(0, 3, false)]
    [InlineData(-1, -1, false)]
    [InlineData(3, 3, false)]
    public void Contains_ReturnsExpectedResult(int x, int y, bool expected)
    {
        var map = new LevelMap(new TileKind[9], 3, 3);
        Assert.Equal(expected, map.Contains(x, y));
    }

    [Fact]
    public void Indexer_ReturnsTileForValidCoordinates()
    {
        var tiles = new[]
        {
            TileKind.Empty, TileKind.Floor, TileKind.Wall,
            TileKind.Connector, TileKind.Empty, TileKind.Floor
        };
        var map = new LevelMap(tiles, 3, 2);

        Assert.Equal(TileKind.Empty, map[0, 0]);
        Assert.Equal(TileKind.Floor, map[1, 0]);
        Assert.Equal(TileKind.Wall, map[2, 0]);
        Assert.Equal(TileKind.Connector, map[0, 1]);
        Assert.Equal(TileKind.Empty, map[1, 1]);
        Assert.Equal(TileKind.Floor, map[2, 1]);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(3, 0)]
    [InlineData(0, 2)]
    public void Indexer_ThrowsArgumentOutOfRangeException_ForInvalidCoordinates(int x, int y)
    {
        var map = new LevelMap(new TileKind[6], 3, 2);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = map[x, y]);
    }

    [Fact]
    public void AsLinearTiles_ReturnsExpectedSequence()
    {
        var tiles = new[]
        {
            TileKind.Empty, TileKind.Floor, TileKind.Wall,
            TileKind.Connector, TileKind.Empty, TileKind.Floor
        };
        var map = new LevelMap(tiles, 3, 2);

        Assert.Equal(tiles, map.AsLinearTiles());
    }
}
