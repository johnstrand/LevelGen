using Xunit;
using LevelGen.Internal;

namespace LevelGen.Tests;

public class PrefabVariantFactoryTests
{
    [Theory]
    // 0 quarter turns, no mirror (Identity)
    [InlineData(1, 2, 4, 5, 0, false, 1, 2)]
    // 1 quarter turn clockwise, no mirror
    // 0 1 2 3      0 1 2 3 4
    // 0 . . . .      0 . . .
    // 1 . . . .      1 . . x
    // 2 . x . .  ->  2 . . .
    // 3 . . . .      3 . . .
    // 4 . . . .
    // Point (1, 2) in 4x5 grid -> (5-1-2, 1) = (2, 1)  (WAIT: (currentHeight - 1 - y, x) => 5-1-2 = 2, 1 is correct)
    [InlineData(1, 2, 4, 5, 1, false, 2, 1)]
    // 2 quarter turns, no mirror
    // (currentWidth - 1 - x, currentHeight - 1 - y) => (4-1-1, 5-1-2) = (2, 2)
    [InlineData(1, 2, 4, 5, 2, false, 2, 2)]
    // 3 quarter turns, no mirror
    // Rotate 1: (5-1-2, 1) = (2, 1) [Grid is 5x4]
    // Rotate 2: (4-1-1, 5-1-2) = (2, 2) [Grid is 4x5]
    // Rotate 3: (5-1-2, 2) = (2, 2) [Grid is 5x4]
    [InlineData(1, 2, 4, 5, 3, false, 2, 2)]

    // 0 quarter turns, mirror horizontally
    // (currentWidth - 1 - x, y) => (4-1-1, 2) = (2, 2)
    [InlineData(1, 2, 4, 5, 0, true, 2, 2)]
    // 1 quarter turn clockwise, mirror horizontally
    // First mirror: (2, 2). Then rotate 1: (5-1-2, 2) = (2, 2)
    [InlineData(1, 2, 4, 5, 1, true, 2, 2)]
    // 2 quarter turns, mirror horizontally
    // First mirror: (2, 2). Rotate 2: (4-1-2, 5-1-2) = (1, 2)
    [InlineData(1, 2, 4, 5, 2, true, 1, 2)]
    // 3 quarter turns, mirror horizontally
    // First mirror: (2, 2) [Grid is 4x5]
    // Rotate 1: (5-1-2, 2) = (2, 2) [Grid is 5x4]
    // Rotate 2: (4-1-2, 5-1-2) = (1, 2) [Grid is 4x5]
    // Rotate 3: (5-1-2, 1) = (2, 1) [Grid is 5x4]
    [InlineData(1, 2, 4, 5, 3, true, 2, 1)]

    // Test a different point to avoid (2,2) overlaps in assertions
    // Point (0, 0) in 3x2 grid
    [InlineData(0, 0, 3, 2, 0, false, 0, 0)]
    // Rotate 1: (2-1-0, 0) = (1, 0)
    [InlineData(0, 0, 3, 2, 1, false, 1, 0)]
    // Rotate 2: (3-1-0, 2-1-0) = (2, 1)
    [InlineData(0, 0, 3, 2, 2, false, 2, 1)]
    // Rotate 3: (0, 3-1-0) = (0, 2)
    [InlineData(0, 0, 3, 2, 3, false, 0, 2)]

    // Mirror: (3-1-0, 0) = (2, 0)
    [InlineData(0, 0, 3, 2, 0, true, 2, 0)]
    // Mirror + Rotate 1: (2, 0) -> (2-1-0, 2) = (1, 2)
    [InlineData(0, 0, 3, 2, 1, true, 1, 2)]
    // Mirror + Rotate 2: (2, 0) -> (3-1-2, 2-1-0) = (0, 1)
    [InlineData(0, 0, 3, 2, 2, true, 0, 1)]
    // Mirror + Rotate 3: (2, 0) -> (0, 3-1-2) = (0, 0)
    [InlineData(0, 0, 3, 2, 3, true, 0, 0)]
    public void TransformPoint_ReturnsExpectedCoordinates(
        int x, int y,
        int width, int height,
        int quarterTurns, bool mirror,
        int expectedX, int expectedY)
    {
        var point = new Point2(x, y);
        var transform = new PrefabTransform(quarterTurns, mirror);

        var result = PrefabVariantFactory.TransformPoint(point, width, height, transform);

        Assert.Equal(new Point2(expectedX, expectedY), result);
    }
}
