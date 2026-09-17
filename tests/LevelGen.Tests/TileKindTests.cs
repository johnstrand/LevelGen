namespace LevelGen.Tests;

public class TileKindTests
{
    [Theory]
    [InlineData(TileKind.Empty, 0)]
    [InlineData(TileKind.Wall, 1)]
    [InlineData(TileKind.Floor, 2)]
    [InlineData(TileKind.Connector, 3)]
    public void UnderlyingValues_MatchExpectedIntegers(TileKind tileKind, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)tileKind);
    }

    [Fact]
    public void DefinedEnumValues_ContainsAllExpectedMembers()
    {
        var values = Enum.GetValues<TileKind>();

        Assert.Equal(4, values.Length);
        Assert.Contains(TileKind.Empty, values);
        Assert.Contains(TileKind.Wall, values);
        Assert.Contains(TileKind.Floor, values);
        Assert.Contains(TileKind.Connector, values);
    }

    [Theory]
    [InlineData("Empty", TileKind.Empty)]
    [InlineData("Wall", TileKind.Wall)]
    [InlineData("Floor", TileKind.Floor)]
    [InlineData("Connector", TileKind.Connector)]
    public void Parse_And_ToString_BehaveAsExpected(string name, TileKind expectedKind)
    {
        var parsed = Enum.Parse<TileKind>(name);
        Assert.Equal(expectedKind, parsed);
        Assert.Equal(name, expectedKind.ToString());
    }
}
