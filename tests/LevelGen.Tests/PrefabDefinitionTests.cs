using System;
using System.Collections.Generic;
using Xunit;

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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhiteSpaceName_ThrowsArgumentException(string? name)
    {
        var width = 2;
        var height = 2;
        var tiles = new[] { TileKind.Wall, TileKind.Wall, TileKind.Wall, TileKind.Wall };

        var exception = Assert.Throws<ArgumentException>(() => new PrefabDefinition(name!, width, height, tiles));
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_NegativeOrZeroWidth_ThrowsArgumentOutOfRangeException(int width)
    {
        var name = "Test";
        var height = 2;
        var tiles = new[] { TileKind.Wall, TileKind.Wall }; // 2 * ? length

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PrefabDefinition(name, width, height, tiles));
        Assert.Equal("width", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_NegativeOrZeroHeight_ThrowsArgumentOutOfRangeException(int height)
    {
        var name = "Test";
        var width = 2;
        var tiles = new[] { TileKind.Wall, TileKind.Wall }; // 2 * ? length

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PrefabDefinition(name, width, height, tiles));
        Assert.Equal("height", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullTiles_ThrowsArgumentNullException()
    {
        var name = "Test";
        var width = 2;
        var height = 2;

        var exception = Assert.Throws<ArgumentNullException>(() => new PrefabDefinition(name, width, height, null!));
        Assert.Equal("tiles", exception.ParamName);
    }

    [Fact]
    public void Constructor_TileCountMismatch_ThrowsArgumentException()
    {
        var name = "Test";
        var width = 2;
        var height = 2;
        var tiles = new[] { TileKind.Wall, TileKind.Wall, TileKind.Wall }; // Only 3 tiles, should be 4

        var exception = Assert.Throws<ArgumentException>(() => new PrefabDefinition(name, width, height, tiles));
        Assert.Equal("tiles", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidArguments_InitializesProperties()
    {
        var name = "Test";
        var width = 2;
        var height = 2;
        var tiles = new[] { TileKind.Wall, TileKind.Floor, TileKind.Floor, TileKind.Wall };
        var doodads = new[] { new PrefabDoodad(new Point2(1, 1), 'x') };

        var definition = new PrefabDefinition(name, width, height, tiles, doodads);

        Assert.Equal(name, definition.Name);
        Assert.Equal(width, definition.Width);
        Assert.Equal(height, definition.Height);

        Assert.Equal(TileKind.Wall, definition[0, 0]);
        Assert.Equal(TileKind.Floor, definition[1, 0]);
        Assert.Equal(TileKind.Floor, definition[0, 1]);
        Assert.Equal(TileKind.Wall, definition[1, 1]);

        Assert.Single(definition.Doodads);
        Assert.Equal(new Point2(1, 1), definition.Doodads[0].Position);
        Assert.Equal('x', definition.Doodads[0].Marker);
    }

    [Fact]
    public void AsLinearTiles_ReturnsTilesInSequence()
    {
        var tiles = new[] { TileKind.Wall, TileKind.Floor, TileKind.Floor, TileKind.Wall };
        var definition = new PrefabDefinition("Test", 2, 2, tiles);

        var result = definition.AsLinearTiles();

        Assert.Equal(tiles.Length, result.Count);
        Assert.Equal(tiles, result);
    }
}
