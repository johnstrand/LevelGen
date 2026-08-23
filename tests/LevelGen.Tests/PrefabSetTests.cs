using System;
using Xunit;
using LevelGen;

namespace LevelGen.Tests;

public sealed class PrefabSetTests
{
    [Fact]
    public void Constructor_NullPrefabs_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>("prefabs", () => new PrefabSet(null!));
    }

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

    [Fact]
    public void GetEnumerator_GenericAndNonGeneric_IteratesOverAllItems()
    {
        // Arrange
        var tiles = new[] { TileKind.Empty };
        var prefab1 = new PrefabDefinition("Prefab1", 1, 1, tiles);
        var prefab2 = new PrefabDefinition("Prefab2", 1, 1, tiles);
        var initialCollection = new[] { prefab1, prefab2 };
        var prefabSet = new PrefabSet(initialCollection);

        // Act - Generic Enumeration
        var enumeratedGeneric = new System.Collections.Generic.List<PrefabDefinition>();
        foreach (var prefab in prefabSet)
        {
            enumeratedGeneric.Add(prefab);
        }

        // Act - Non-Generic Explicit Enumeration
        System.Collections.IEnumerable nonGenericEnumerable = prefabSet;
        var enumeratedNonGeneric = new System.Collections.Generic.List<object?>();
        var enumerator = nonGenericEnumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumeratedNonGeneric.Add(enumerator.Current);
        }

        // Assert
        Assert.Equal(2, prefabSet.Count);
        Assert.Equal(initialCollection, enumeratedGeneric);
        Assert.Equal(initialCollection, enumeratedNonGeneric);
    }

    [Fact]
    public void GetEnumerator_EmptySet_YieldsNoItems()
    {
        // Arrange
        var emptySet = new PrefabSet(Array.Empty<PrefabDefinition>());

        // Act & Assert
        Assert.Empty(emptySet);

        System.Collections.IEnumerable nonGenericEnumerable = emptySet;
        var enumerator = nonGenericEnumerable.GetEnumerator();
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void IndexerAndCount_ReturnsExpectedPrefabs()
    {
        // Arrange
        var tiles = new[] { TileKind.Empty };
        var prefab1 = new PrefabDefinition("Prefab1", 1, 1, tiles);
        var prefab2 = new PrefabDefinition("Prefab2", 1, 1, tiles);
        var prefabSet = new PrefabSet(new[] { prefab1, prefab2 });

        // Act & Assert
        Assert.Equal(2, prefabSet.Count);
        Assert.Same(prefab1, prefabSet[0]);
        Assert.Same(prefab2, prefabSet[1]);
    }
}
