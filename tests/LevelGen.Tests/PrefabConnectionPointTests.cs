namespace LevelGen.Tests;

public class PrefabConnectionPointTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var position = new Point2(3, 4);
        var facing = Direction.North;

        // Act
        var connectionPoint = new PrefabConnectionPoint(position, facing);

        // Assert
        Assert.Equal(position, connectionPoint.Position);
        Assert.Equal(facing, connectionPoint.Facing);
    }

    [Fact]
    public void Deconstruct_ReturnsPositionAndFacing()
    {
        // Arrange
        var position = new Point2(5, 6);
        var facing = Direction.East;
        var connectionPoint = new PrefabConnectionPoint(position, facing);

        // Act
        var (deconstructedPosition, deconstructedFacing) = connectionPoint;

        // Assert
        Assert.Equal(position, deconstructedPosition);
        Assert.Equal(facing, deconstructedFacing);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        // Arrange
        var point1 = new PrefabConnectionPoint(new Point2(1, 2), Direction.South);
        var point2 = new PrefabConnectionPoint(new Point2(1, 2), Direction.South);

        // Act & Assert
        Assert.Equal(point1, point2);
        Assert.True(point1 == point2);
        Assert.False(point1 != point2);
        Assert.True(point1.Equals(point2));
        Assert.True(point1.Equals((object)point2));
        Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var basePoint = new PrefabConnectionPoint(new Point2(1, 2), Direction.South);
        var differentPosition = new PrefabConnectionPoint(new Point2(2, 2), Direction.South);
        var differentFacing = new PrefabConnectionPoint(new Point2(1, 2), Direction.West);

        // Act & Assert
        Assert.NotEqual(basePoint, differentPosition);
        Assert.False(basePoint == differentPosition);
        Assert.True(basePoint != differentPosition);

        Assert.NotEqual(basePoint, differentFacing);
        Assert.False(basePoint == differentFacing);
        Assert.True(basePoint != differentFacing);
    }

    [Fact]
    public void WithExpression_UpdatesSpecifiedProperty()
    {
        // Arrange
        var original = new PrefabConnectionPoint(new Point2(1, 2), Direction.North);

        // Act
        var updatedFacing = original with { Facing = Direction.East };
        var updatedPosition = original with { Position = new Point2(10, 20) };

        // Assert
        Assert.Equal(new Point2(1, 2), updatedFacing.Position);
        Assert.Equal(Direction.East, updatedFacing.Facing);

        Assert.Equal(new Point2(10, 20), updatedPosition.Position);
        Assert.Equal(Direction.North, updatedPosition.Facing);
    }
}
