using System;
using System.Linq;
using LevelGen;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public class BlocksPrefabParserCoreTests
{
    [Fact]
    public void Parse_ValidSinglePrefab_ParsesCorrectly()
    {
        // Arrange
        var input = TestPrefabs.SinglePrefabWithComment;

        // Act
        var prefabSet = BlocksPrefabParserCore.Parse(input);

        // Assert
        Assert.NotNull(prefabSet);
        Assert.Single(prefabSet);

        var prefab = prefabSet[0];
        Assert.NotNull(prefab);
        Assert.Equal("Room1", prefab.Name);
        Assert.Equal(3, prefab.Width);
        Assert.Equal(3, prefab.Height);
        Assert.Empty(prefab.Doodads);

        var expectedTiles = new[]
        {
            TileKind.Wall, TileKind.Wall, TileKind.Wall,
            TileKind.Wall, TileKind.Floor, TileKind.Wall,
            TileKind.Wall, TileKind.Wall, TileKind.Wall
        };
        Assert.Equal(expectedTiles, prefab.AsLinearTiles().ToArray());
    }

    [Fact]
    public void Parse_AllTileKindsAndDoodads_ParsesCorrectly()
    {
        // Arrange
        var input = TestPrefabs.AllTokensAndDoodadsRoom;

        // Act
        var prefabSet = BlocksPrefabParserCore.Parse(input);

        // Assert
        var prefab = prefabSet[0];
        Assert.Equal(8, prefab.Width);
        Assert.Equal(1, prefab.Height);

        var expectedTiles = new[]
        {
            TileKind.Wall,
            TileKind.Floor,
            TileKind.Connector,
            TileKind.Empty,
            TileKind.Floor, // ?
            TileKind.Floor, // A
            TileKind.Empty, // ' '
            TileKind.Floor  // P
        };
        Assert.Equal(expectedTiles, prefab.AsLinearTiles().ToArray());

        Assert.Equal(3, prefab.Doodads.Count);
        Assert.Equal(new Point2(4, 0), prefab.Doodads[0].Position);
        Assert.Equal('?', prefab.Doodads[0].Marker);

        Assert.Equal(new Point2(5, 0), prefab.Doodads[1].Position);
        Assert.Equal('A', prefab.Doodads[1].Marker);

        Assert.Equal(new Point2(7, 0), prefab.Doodads[2].Position);
        Assert.Equal('P', prefab.Doodads[2].Marker);
    }

    [Fact]
    public void Parse_UnevenRowLengths_PadsWithEmptyTiles()
    {
        // Arrange
        var input = TestPrefabs.UnevenRowsRoom;

        // Act
        var prefabSet = BlocksPrefabParserCore.Parse(input);

        // Assert
        var prefab = prefabSet[0];
        Assert.Equal(5, prefab.Width);
        Assert.Equal(3, prefab.Height);

        // Row 0: ###   (3 chars + 2 padded spaces)
        // Row 1: #     (1 char + 4 padded spaces)
        // Row 2: ##### (5 chars)
        var expectedTiles = new[]
        {
            TileKind.Wall, TileKind.Wall, TileKind.Wall, TileKind.Empty, TileKind.Empty,
            TileKind.Wall, TileKind.Empty, TileKind.Empty, TileKind.Empty, TileKind.Empty,
            TileKind.Wall, TileKind.Wall, TileKind.Wall, TileKind.Wall, TileKind.Wall
        };
        Assert.Equal(expectedTiles, prefab.AsLinearTiles().ToArray());
    }

    [Fact]
    public void Parse_MultiplePrefabs_ParsesAll()
    {
        // Arrange
        var input = TestPrefabs.MultiplePrefabsWithComments;

        // Act
        var prefabSet = BlocksPrefabParserCore.Parse(input);

        // Assert
        Assert.Equal(2, prefabSet.Count);

        var first = prefabSet.First(p => p.Name == "First");
        var second = prefabSet.First(p => p.Name == "Second");

        Assert.Equal(2, first.Width);
        Assert.Equal(1, first.Height);
        Assert.Equal(2, second.Width);
        Assert.Equal(2, second.Height);
    }

    [Fact]
    public void Parse_CarriageReturnsInInput_HandledCorrectly()
    {
        // Arrange
        var input = TestPrefabs.CarriageReturnRoom;

        // Act
        var prefabSet = BlocksPrefabParserCore.Parse(input);

        // Assert
        var prefab = prefabSet[0];
        Assert.Equal(2, prefab.Width);
        Assert.Equal(2, prefab.Height);
    }

    [Fact]
    public void Parse_TilesBeforeHeader_ThrowsFormatException()
    {
        // Arrange
        var input = TestPrefabs.TilesBeforeHeader;

        // Act & Assert
        var ex = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(input));
        Assert.Contains("Encountered prefab tiles before a section header", ex.Message);
    }

    [Theory]
    [InlineData("> ")]
    [InlineData(">\t")]
    [InlineData(">\r\n")]
    public void Parse_BlankPrefabName_ThrowsFormatException(string input)
    {
        // Act & Assert
        var ex = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(input));
        Assert.Contains("Prefab names cannot be blank.", ex.Message);
    }

    [Fact]
    public void Parse_UnsupportedToken_ThrowsFormatException()
    {
        // Arrange
        var input = TestPrefabs.UnsupportedTileToken;

        // Act & Assert
        var ex = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(input));
        Assert.Contains("Unsupported tile token '+' in prefab 'Invalid'.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("// Only comments\n// Nothing else")]
    [InlineData("> HeaderOnlyWithNoRows\n\n")]
    public void Parse_NoPrefabsFound_ThrowsFormatException(string input)
    {
        // Act & Assert
        var ex = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(input));
        Assert.Contains("No prefabs were found", ex.Message);
    }
}
