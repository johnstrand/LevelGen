using LevelGen;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public class DirectionExtensionsTests
{
    [Theory]
    // Zero rotation
    [InlineData(Direction.North, 0, Direction.North)]
    [InlineData(Direction.East, 0, Direction.East)]
    [InlineData(Direction.South, 0, Direction.South)]
    [InlineData(Direction.West, 0, Direction.West)]

    // Positive rotations
    [InlineData(Direction.North, 1, Direction.East)]
    [InlineData(Direction.North, 2, Direction.South)]
    [InlineData(Direction.North, 3, Direction.West)]
    [InlineData(Direction.North, 4, Direction.North)]
    [InlineData(Direction.East, 1, Direction.South)]
    [InlineData(Direction.East, 2, Direction.West)]
    [InlineData(Direction.East, 3, Direction.North)]
    [InlineData(Direction.East, 4, Direction.East)]

    // Negative rotations
    [InlineData(Direction.North, -1, Direction.West)]
    [InlineData(Direction.North, -2, Direction.South)]
    [InlineData(Direction.North, -3, Direction.East)]
    [InlineData(Direction.North, -4, Direction.North)]
    [InlineData(Direction.East, -1, Direction.North)]

    // Large values
    [InlineData(Direction.North, 400, Direction.North)]
    [InlineData(Direction.North, -400, Direction.North)]
    [InlineData(Direction.North, 401, Direction.East)]
    [InlineData(Direction.North, -401, Direction.West)]
    public void RotateClockwise_ReturnsExpectedDirection(Direction initialDirection, int quarterTurns, Direction expectedDirection)
    {
        // Act
        var result = initialDirection.RotateClockwise(quarterTurns);

        // Assert
        Assert.Equal(expectedDirection, result);
    }
}
