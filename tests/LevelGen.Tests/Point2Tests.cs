namespace LevelGen.Tests;

public sealed class Point2Tests
{
    [Fact]
    public void Addition_ReturnsCorrectResult()
    {
        var point1 = new Point2(1, 2);
        var point2 = new Point2(3, 4);

        var result = point1 + point2;

        Assert.Equal(new Point2(4, 6), result);
    }

    [Fact]
    public void Addition_WithNegativeNumbers_ReturnsCorrectResult()
    {
        var point1 = new Point2(-1, -2);
        var point2 = new Point2(3, -4);

        var result = point1 + point2;

        Assert.Equal(new Point2(2, -6), result);
    }

    [Fact]
    public void Addition_WithZero_ReturnsSamePoint()
    {
        var point1 = new Point2(1, 2);
        var point2 = new Point2(0, 0);

        var result = point1 + point2;

        Assert.Equal(point1, result);
    }

    [Fact]
    public void Subtraction_ReturnsCorrectResult()
    {
        var point1 = new Point2(5, 7);
        var point2 = new Point2(3, 4);

        var result = point1 - point2;

        Assert.Equal(new Point2(2, 3), result);
    }

    [Fact]
    public void Subtraction_WithNegativeNumbers_ReturnsCorrectResult()
    {
        var point1 = new Point2(-1, -2);
        var point2 = new Point2(3, -4);

        var result = point1 - point2;

        Assert.Equal(new Point2(-4, 2), result);
    }

    [Fact]
    public void Subtraction_WithZero_ReturnsSamePoint()
    {
        var point1 = new Point2(1, 2);
        var point2 = new Point2(0, 0);

        var result = point1 - point2;

        Assert.Equal(point1, result);
    }
}
