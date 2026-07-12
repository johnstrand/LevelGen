using System;
using Xunit;
using LevelGen;

namespace LevelGen.Tests;

public sealed class PrefabTransformTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 0)]
    [InlineData(5, 1)]
    [InlineData(8, 0)]
    [InlineData(-1, 3)]
    [InlineData(-2, 2)]
    [InlineData(-3, 1)]
    [InlineData(-4, 0)]
    [InlineData(-5, 3)]
    [InlineData(-8, 0)]
    public void Constructor_NormalizesQuarterTurns(int input, int expected)
    {
        var transform = new PrefabTransform(input, false);
        Assert.Equal(expected, transform.QuarterTurnsClockwise);
    }
}
