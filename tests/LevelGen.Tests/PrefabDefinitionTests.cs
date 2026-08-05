namespace LevelGen.Tests;

public sealed class PrefabDefinitionTests
{
    private static PrefabDefinition CreateTestPrefab()
    {
        var tiles = new TileKind[]
        {
            TileKind.Wall, TileKind.Floor, TileKind.Wall,
            TileKind.Floor, TileKind.Empty, TileKind.Floor,
        };

        return new PrefabDefinition("TestPrefab", 3, 2, tiles);
    }

    [Theory]
    [InlineData(0, 0, TileKind.Wall)]
    [InlineData(1, 0, TileKind.Floor)]
    [InlineData(2, 0, TileKind.Wall)]
    [InlineData(0, 1, TileKind.Floor)]
    [InlineData(1, 1, TileKind.Empty)]
    [InlineData(2, 1, TileKind.Floor)]
    public void Indexer_InBounds_ReturnsCorrectTile(int x, int y, TileKind expectedTile)
    {
        var prefab = CreateTestPrefab();

        var tile = prefab[x, y];

        Assert.Equal(expectedTile, tile);
    }

    [Theory]
    [InlineData(-1, 0, "x")]
    [InlineData(3, 0, "x")]
    [InlineData(0, -1, "y")]
    [InlineData(0, 2, "y")]
    public void Indexer_OutOfBounds_ThrowsArgumentOutOfRangeException(int x, int y, string expectedParamName)
    {
        var prefab = CreateTestPrefab();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => prefab[x, y]);

        Assert.Equal(expectedParamName, exception.ParamName);
    }
}
