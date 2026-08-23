using LevelGen.Blocks;
using Xunit;

namespace LevelGen.Tests;

public sealed class BlocksPrefabParserTests
{
    [Fact]
    public void Parse_TilesBeforeSectionHeader_ThrowsFormatException()
    {
        const string input = "..##\n> Name";

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("Encountered prefab tiles before a section header: '..##'.", exception.Message);
    }

    [Fact]
    public void Parse_WithTilesBeforeHeader_ThrowsFormatException()
    {
        var input = """
            #.
            > TestRoom
            #.
            """;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("Encountered prefab tiles before a section header", exception.Message);
    }

    [Fact]
    public void Parse_BlankPrefabName_ThrowsFormatException()
    {
        const string input = ">\n..##";

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("Prefab names cannot be blank.", exception.Message);
    }

    [Fact]
    public void Parse_WithBlankPrefabName_ThrowsFormatException()
    {
        var input = """
            >
            #.
            """;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("Prefab names cannot be blank", exception.Message);
    }

    [Fact]
    public void Parse_UnsupportedTileToken_ThrowsFormatException()
    {
        const string input = "> Room\n..+";

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("Unsupported tile token '+' in prefab 'Room'.", exception.Message);
    }

    [Fact]
    public void Parse_WithUnsupportedTileToken_ThrowsFormatException()
    {
        var input = """
            > TestRoom
            #.
            #+
            """;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("Unsupported tile token '+' in prefab 'TestRoom'", exception.Message);
    }

    [Fact]
    public void Parse_NoPrefabsFound_ThrowsFormatException()
    {
        const string input = "// Just a comment line";

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));

        Assert.Contains("No prefabs were found in the supplied BlocksPrefabParser input.", exception.Message);
    }

    [Fact]
    public void Parse_WithNoPrefabs_ThrowsFormatException()
    {
        var input = """
            / Just a comment line
            """;

        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParser.Parse(input));
        Assert.Contains("No prefabs were found", exception.Message);
    }
}
