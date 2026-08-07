using LevelGen;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public class TileKindExtensionsTests
{
    [Theory]
    [InlineData(TileKind.Empty, false)]
    [InlineData(TileKind.Wall, false)]
    [InlineData(TileKind.Floor, true)]
    [InlineData(TileKind.Connector, true)]
    [InlineData((TileKind)999, false)]
    public void IsWalkable_ReturnsExpectedResult(TileKind kind, bool expected)
    {
        // Act
        var result = kind.IsWalkable();

        // Assert
        Assert.Equal(expected, result);
    }
}
