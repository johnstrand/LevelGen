using LevelGen.Internal;

namespace LevelGen.Tests;

public class BlocksPrefabParserCoreTests
{
    [Fact]
    public void Parse_WithEmptyInput_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(""));
        Assert.Contains("No prefabs were found in the supplied BlocksPrefabParser input.", exception.Message);
    }

    [Fact]
    public void Parse_WithOnlyComments_ThrowsFormatException()
    {
        var input = "// This is a comment\n// Another comment";
        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(input));
        Assert.Contains("No prefabs were found in the supplied BlocksPrefabParser input.", exception.Message);
    }

    [Fact]
    public void Parse_TilesBeforeHeader_ThrowsFormatException()
    {
        var input = ".#.\n> Header";
        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(input));
        Assert.Contains("Encountered prefab tiles before a section header", exception.Message);
    }

    [Fact]
    public void Parse_BlankPrefabName_ThrowsFormatException()
    {
        var input = "> \n.#.";
        var exception = Assert.Throws<FormatException>(() => BlocksPrefabParserCore.Parse(input));
        Assert.Contains("Prefab names cannot be blank.", exception.Message);
    }
}
