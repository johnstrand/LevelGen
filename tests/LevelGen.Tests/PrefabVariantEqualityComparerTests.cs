using System;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public sealed class PrefabVariantEqualityComparerTests
{
    private static PrefabDefinition CreateTestPrefab(string name = "TestPrefab")
    {
        return new PrefabDefinition(name, 1, 1, [TileKind.Wall]);
    }

    private static PrefabVariant CreateVariant(
        int width = 2,
        int height = 2,
        TileKind[]? tiles = null,
        PrefabConnectionPoint[]? connections = null,
        PrefabDoodad[]? doodads = null,
        PrefabDefinition? source = null,
        PrefabTransform? transform = null)
    {
        return new PrefabVariant(
            source ?? CreateTestPrefab(),
            transform ?? PrefabTransform.Identity,
            width,
            height,
            tiles ?? [TileKind.Floor, TileKind.Wall, TileKind.Floor, TileKind.Wall],
            connections ?? [new PrefabConnectionPoint(new Point2(0, 0), Direction.North)],
            doodads ?? []);
    }

    [Fact]
    public void Instance_ReturnsNonNullSingleton()
    {
        var instance1 = PrefabVariantEqualityComparer.Instance;
        var instance2 = PrefabVariantEqualityComparer.Instance;

        Assert.NotNull(instance1);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Equals_SameReferenceOrBothNull_ReturnsTrue()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant = CreateVariant();

        Assert.True(comparer.Equals(null, null));
        Assert.True(comparer.Equals(variant, variant));
    }

    [Fact]
    public void Equals_OneNull_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant = CreateVariant();

        Assert.False(comparer.Equals(variant, null));
        Assert.False(comparer.Equals(null, variant));
    }

    [Fact]
    public void Equals_IdenticalVariants_ReturnsTrue()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant();
        var variant2 = CreateVariant();

        Assert.True(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentWidth_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(width: 2);
        var variant2 = CreateVariant(width: 3);

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentHeight_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(height: 2);
        var variant2 = CreateVariant(height: 3);

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentTileCount_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(tiles: [TileKind.Floor, TileKind.Wall]);
        var variant2 = CreateVariant(tiles: [TileKind.Floor, TileKind.Wall, TileKind.Floor]);

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentTiles_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(tiles: [TileKind.Floor, TileKind.Wall]);
        var variant2 = CreateVariant(tiles: [TileKind.Floor, TileKind.Floor]);

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentConnectionCount_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(connections: [new PrefabConnectionPoint(new Point2(0, 0), Direction.North)]);
        var variant2 = CreateVariant(connections: []);

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentConnections_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(connections: [new PrefabConnectionPoint(new Point2(0, 0), Direction.North)]);
        var variant2 = CreateVariant(connections: [new PrefabConnectionPoint(new Point2(0, 0), Direction.South)]);

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentSourceTransformOrDoodads_ReturnsTrue()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;

        var source1 = CreateTestPrefab("Prefab1");
        var source2 = CreateTestPrefab("Prefab2");

        var variant1 = CreateVariant(
            source: source1,
            transform: PrefabTransform.Identity,
            doodads: [new PrefabDoodad(new Point2(0, 0), 'A')]);

        var variant2 = CreateVariant(
            source: source2,
            transform: new PrefabTransform(1, true),
            doodads: [new PrefabDoodad(new Point2(1, 1), 'B')]);

        Assert.True(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_InvalidTileKindInFirstVariant_ThrowsArgumentOutOfRangeException()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(tiles: [(TileKind)999]);
        var variant2 = CreateVariant(tiles: [TileKind.Floor]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => comparer.Equals(variant1, variant2));
        Assert.Equal("tileKind", exception.ParamName);
    }

    [Fact]
    public void Equals_InvalidTileKindInSecondVariant_ThrowsArgumentOutOfRangeException()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(tiles: [TileKind.Floor]);
        var variant2 = CreateVariant(tiles: [(TileKind)999]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => comparer.Equals(variant1, variant2));
        Assert.Equal("tileKind", exception.ParamName);
    }

    [Fact]
    public void GetHashCode_NullObj_ThrowsArgumentNullException()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;

        Assert.Throws<ArgumentNullException>(() => comparer.GetHashCode(null!));
    }

    [Fact]
    public void GetHashCode_EqualVariants_ProduceSameHashCode()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant();
        var variant2 = CreateVariant();

        Assert.Equal(comparer.GetHashCode(variant1), comparer.GetHashCode(variant2));
    }

    [Fact]
    public void GetHashCode_InvalidTileKind_ThrowsArgumentOutOfRangeException()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant = CreateVariant(tiles: [(TileKind)999]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => comparer.GetHashCode(variant));
        Assert.Equal("tileKind", exception.ParamName);
    }
}
