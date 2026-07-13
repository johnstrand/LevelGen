using LevelGen;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public class GeneratorCoreTests
{
    [Fact]
    public void AllDirections_ContainsAllDirections()
    {
        // Assert
        Assert.Equal(Enum.GetValues<Direction>(), GeneratorCore.AllDirections);
    }
}
