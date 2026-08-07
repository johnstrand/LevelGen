using System;
using Xunit;
using LevelGen;

namespace LevelGen.Tests;

public sealed class PrefabSetTests
{
    [Fact]
    public void Constructor_DuplicateNames_ThrowsArgumentException()
    {
        // Arrange
        var tiles = new[] { TileKind.Empty };
        var prefab1 = new PrefabDefinition("MyPrefab", 1, 1, tiles);
        var prefab2 = new PrefabDefinition("MyPrefab", 1, 1, tiles);
        var prefabs = new[] { prefab1, prefab2 };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>("prefabs", () => new PrefabSet(prefabs));
        Assert.Contains("Duplicate: 'MyPrefab'", exception.Message);
    }
}
