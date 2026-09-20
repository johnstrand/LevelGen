using System;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public sealed class PrefabVariantEqualityComparerTests
{
    private static PrefabDefinition CreateDummyPrefab()
    {
        return new PrefabDefinition("TestPrefab", 1, 1, new[] { TileKind.Floor });
    }

    private static PrefabVariant CreateVariant(
        int width = 1,
        int height = 1,
        TileKind[]? tiles = null,
        PrefabConnectionPoint[]? connections = null)
    {
        return new PrefabVariant(
            CreateDummyPrefab(),
            PrefabTransform.Identity,
            width,
            height,
            tiles ?? new[] { TileKind.Floor },
            connections ?? Array.Empty<PrefabConnectionPoint>(),
            Array.Empty<PrefabDoodad>());
    }

    [Fact]
    public void Instance_ReturnsNonNullSingletonInstance()
    {
        var instance1 = PrefabVariantEqualityComparer.Instance;
        var instance2 = PrefabVariantEqualityComparer.Instance;

        Assert.NotNull(instance1);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant = CreateVariant();

        Assert.True(comparer.Equals(variant, variant));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;

        Assert.True(comparer.Equals(null, null));
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
    public void Equals_DifferentWidthOrHeight_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var baseVariant = CreateVariant(width: 2, height: 2, tiles: new[] { TileKind.Floor, TileKind.Floor, TileKind.Floor, TileKind.Floor });
        var diffWidth = CreateVariant(width: 3, height: 2, tiles: new[] { TileKind.Floor, TileKind.Floor, TileKind.Floor, TileKind.Floor, TileKind.Floor, TileKind.Floor });
        var diffHeight = CreateVariant(width: 2, height: 3, tiles: new[] { TileKind.Floor, TileKind.Floor, TileKind.Floor, TileKind.Floor, TileKind.Floor, TileKind.Floor });

        Assert.False(comparer.Equals(baseVariant, diffWidth));
        Assert.False(comparer.Equals(baseVariant, diffHeight));
    }

    [Fact]
    public void Equals_DifferentTileOrConnectionCount_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(width: 1, height: 2, tiles: new[] { TileKind.Floor, TileKind.Wall });
        var variant2 = CreateVariant(width: 1, height: 1, tiles: new[] { TileKind.Floor });
        var conn = new PrefabConnectionPoint(new Point2(0, 0), Direction.North);
        var variantWithConn = CreateVariant(width: 1, height: 1, connections: new[] { conn });
        var variantWithoutConn = CreateVariant(width: 1, height: 1, connections: Array.Empty<PrefabConnectionPoint>());

        Assert.False(comparer.Equals(variant1, variant2));
        Assert.False(comparer.Equals(variantWithConn, variantWithoutConn));
    }

    [Fact]
    public void Equals_DifferentTileContent_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(tiles: new[] { TileKind.Floor });
        var variant2 = CreateVariant(tiles: new[] { TileKind.Wall });

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentConnectionContent_ReturnsFalse()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var conn1 = new PrefabConnectionPoint(new Point2(0, 0), Direction.North);
        var conn2 = new PrefabConnectionPoint(new Point2(0, 0), Direction.South);

        var variant1 = CreateVariant(connections: new[] { conn1 });
        var variant2 = CreateVariant(connections: new[] { conn2 });

        Assert.False(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_IdenticalVariants_ReturnsTrue()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var conn = new PrefabConnectionPoint(new Point2(0, 0), Direction.North);
        var variant1 = CreateVariant(width: 2, height: 1, tiles: new[] { TileKind.Floor, TileKind.Wall }, connections: new[] { conn });
        var variant2 = CreateVariant(width: 2, height: 1, tiles: new[] { TileKind.Floor, TileKind.Wall }, connections: new[] { conn });

        Assert.True(comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_InvalidTileKind_ThrowsArgumentOutOfRangeException()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var invalidTileVariant = CreateVariant(tiles: new[] { (TileKind)999 });
        var validVariant = CreateVariant(tiles: new[] { TileKind.Floor });

        Assert.Throws<ArgumentOutOfRangeException>(() => comparer.Equals(invalidTileVariant, validVariant));
        Assert.Throws<ArgumentOutOfRangeException>(() => comparer.Equals(validVariant, invalidTileVariant));
    }

    [Fact]
    public void GetHashCode_NullObject_ThrowsArgumentNullException()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;

        Assert.Throws<ArgumentNullException>(() => comparer.GetHashCode(null!));
    }

    [Fact]
    public void GetHashCode_EqualObjects_ReturnSameHashCode()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var conn = new PrefabConnectionPoint(new Point2(0, 0), Direction.East);
        var variant1 = CreateVariant(width: 2, height: 1, tiles: new[] { TileKind.Floor, TileKind.Connector }, connections: new[] { conn });
        var variant2 = CreateVariant(width: 2, height: 1, tiles: new[] { TileKind.Floor, TileKind.Connector }, connections: new[] { conn });

        Assert.True(comparer.Equals(variant1, variant2));
        Assert.Equal(comparer.GetHashCode(variant1), comparer.GetHashCode(variant2));
    }

    [Fact]
    public void GetHashCode_InvalidTileKind_ThrowsArgumentOutOfRangeException()
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var invalidTileVariant = CreateVariant(tiles: new[] { (TileKind)999 });

        Assert.Throws<ArgumentOutOfRangeException>(() => comparer.GetHashCode(invalidTileVariant));
    }

    [Theory]
    [InlineData(TileKind.Empty)]
    [InlineData(TileKind.Wall)]
    [InlineData(TileKind.Floor)]
    [InlineData(TileKind.Connector)]
    public void GetHashCode_ValidTileKinds_ExecutesWithoutThrowingAndIsConsistent(TileKind tileKind)
    {
        var comparer = PrefabVariantEqualityComparer.Instance;
        var variant1 = CreateVariant(tiles: new[] { tileKind });
        var variant2 = CreateVariant(tiles: new[] { tileKind });

        var hashCode1 = comparer.GetHashCode(variant1);
        var hashCode2 = comparer.GetHashCode(variant2);

        Assert.Equal(hashCode1, hashCode2);
    }
}
