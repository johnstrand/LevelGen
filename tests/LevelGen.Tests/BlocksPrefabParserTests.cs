using LevelGen.Blocks;
using Xunit;

namespace LevelGen.Tests;

public sealed class BlocksPrefabParserTests
{
    [Fact]
    public void Parse_TilesBeforeSectionHeader_ThrowsFormatException()
    {
        var input = TestPrefabs.TilesBeforeHeaderShort;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("Encountered prefab tiles before a section header: '..##'.", exception.Message);
    }

    [Fact]
    public void Parse_WithTilesBeforeHeader_ThrowsFormatException()
    {
        var input = TestPrefabs.TilesBeforeHeaderTestRoom;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("Encountered prefab tiles before a section header", exception.Message);
    }

    [Fact]
    public void Parse_BlankPrefabName_ThrowsFormatException()
    {
        var input = TestPrefabs.BlankHeaderFollowedByTiles;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("Prefab names cannot be blank.", exception.Message);
    }

    [Fact]
    public void Parse_WithBlankPrefabName_ThrowsFormatException()
    {
        var input = TestPrefabs.BlankHeaderInTextBlock;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("Prefab names cannot be blank", exception.Message);
    }

    [Fact]
    public void Parse_UnsupportedTileToken_ThrowsFormatException()
    {
        var input = TestPrefabs.RoomWithUnsupportedToken;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("Unsupported tile token '+' in prefab 'Room'.", exception.Message);
    }

    [Fact]
    public void Parse_WithUnsupportedTileToken_ThrowsFormatException()
    {
        var input = TestPrefabs.TestRoomWithUnsupportedToken;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("Unsupported tile token '+' in prefab 'TestRoom'", exception.Message);
    }

    [Fact]
    public void Parse_NoPrefabsFound_ThrowsFormatException()
    {
        var input = TestPrefabs.CommentOnlyInput;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("No prefabs were found in the supplied BlocksPrefabParser input.", exception.Message);
    }

    [Fact]
    public void Parse_WithNoPrefabs_ThrowsFormatException()
    {
        var input = TestPrefabs.SingleSlashCommentInput;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("No prefabs were found", exception.Message);
    }
}
