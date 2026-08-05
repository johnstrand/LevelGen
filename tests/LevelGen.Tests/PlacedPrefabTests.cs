namespace LevelGen.Tests;

public class PlacedPrefabTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var prefabName = "TestRoom";
        var origin = new Point2(10, 20);
        var transform = new PrefabTransform(1, true);
        var width = 5;
        var height = 5;
        var isCorridor = true;

        // Act
        var placedPrefab = new PlacedPrefab(prefabName, origin, transform, width, height, isCorridor);

        // Assert
        Assert.Equal(prefabName, placedPrefab.PrefabName);
        Assert.Equal(origin, placedPrefab.Origin);
        Assert.Equal(transform, placedPrefab.Transform);
        Assert.Equal(width, placedPrefab.Width);
        Assert.Equal(height, placedPrefab.Height);
        Assert.True(placedPrefab.IsCorridor);
    }

    [Fact]
    public void Constructor_SetsDefaultIsCorridorToFalse()
    {
        // Arrange
        var prefabName = "TestRoom";
        var origin = new Point2(10, 20);
        var transform = new PrefabTransform(1, true);
        var width = 5;
        var height = 5;

        // Act
        var placedPrefab = new PlacedPrefab(prefabName, origin, transform, width, height);

        // Assert
        Assert.False(placedPrefab.IsCorridor);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        // Arrange
        var prefab1 = new PlacedPrefab("Room", new Point2(0, 0), PrefabTransform.Identity, 10, 10, false);
        var prefab2 = new PlacedPrefab("Room", new Point2(0, 0), PrefabTransform.Identity, 10, 10, false);

        // Act & Assert
        Assert.Equal(prefab1, prefab2);
        Assert.True(prefab1 == prefab2);
        Assert.Equal(prefab1.GetHashCode(), prefab2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var prefab1 = new PlacedPrefab("Room1", new Point2(0, 0), PrefabTransform.Identity, 10, 10, false);
        var prefab2 = new PlacedPrefab("Room2", new Point2(0, 0), PrefabTransform.Identity, 10, 10, false);
        var prefab3 = new PlacedPrefab("Room1", new Point2(1, 0), PrefabTransform.Identity, 10, 10, false);
        var prefab4 = new PlacedPrefab("Room1", new Point2(0, 0), new PrefabTransform(1, false), 10, 10, false);
        var prefab5 = new PlacedPrefab("Room1", new Point2(0, 0), PrefabTransform.Identity, 11, 10, false);
        var prefab6 = new PlacedPrefab("Room1", new Point2(0, 0), PrefabTransform.Identity, 10, 11, false);
        var prefab7 = new PlacedPrefab("Room1", new Point2(0, 0), PrefabTransform.Identity, 10, 10, true);

        // Act & Assert
        Assert.NotEqual(prefab1, prefab2);
        Assert.True(prefab1 != prefab2);
        Assert.NotEqual(prefab1, prefab3);
        Assert.NotEqual(prefab1, prefab4);
        Assert.NotEqual(prefab1, prefab5);
        Assert.NotEqual(prefab1, prefab6);
        Assert.NotEqual(prefab1, prefab7);
    }
}
