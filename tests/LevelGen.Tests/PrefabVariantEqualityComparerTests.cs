using System;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public sealed class PrefabVariantEqualityComparerTests
{
    private readonly PrefabVariantEqualityComparer _comparer = PrefabVariantEqualityComparer.Instance;

    private static PrefabVariant CreateVariant(
        int width = 1,
        int height = 1,
        TileKind[]? tiles = null,
        PrefabConnectionPoint[]? connections = null,
        PrefabDoodad[]? doodads = null)
    {
        var source = new PrefabDefinition("TestPrefab", 1, 1, new[] { TileKind.Wall });
        return new PrefabVariant(
            source,
            PrefabTransform.Identity,
            width,
            height,
            tiles ?? new[] { TileKind.Wall },
            connections ?? Array.Empty<PrefabConnectionPoint>(),
            doodads ?? Array.Empty<PrefabDoodad>());
    }

    [Fact]
    public void Instance_NotNull()
    {
        Assert.NotNull(PrefabVariantEqualityComparer.Instance);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var variant = CreateVariant();
        Assert.True(_comparer.Equals(variant, variant));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        Assert.True(_comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_OneNull_ReturnsFalse()
    {
        var variant = CreateVariant();
        Assert.False(_comparer.Equals(variant, null));
        Assert.False(_comparer.Equals(null, variant));
    }

    [Theory]
    [InlineData(1, 1, 2, 1)]
    [InlineData(1, 1, 1, 2)]
    public void Equals_DifferentDimensions_ReturnsFalse(int w1, int h1, int w2, int h2)
    {
        var variant1 = CreateVariant(width: w1, height: h1);
        var variant2 = CreateVariant(width: w2, height: h2);

        Assert.False(_comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentTileCount_ReturnsFalse()
    {
        var variant1 = CreateVariant(tiles: new[] { TileKind.Wall });
        var variant2 = CreateVariant(tiles: new[] { TileKind.Wall, TileKind.Floor });

        Assert.False(_comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentConnectionCount_ReturnsFalse()
    {
        var conn = new PrefabConnectionPoint(new Point2(0, 0), Direction.North);
        var variant1 = CreateVariant(connections: Array.Empty<PrefabConnectionPoint>());
        var variant2 = CreateVariant(connections: new[] { conn });

        Assert.False(_comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentTileKinds_ReturnsFalse()
    {
        var variant1 = CreateVariant(tiles: new[] { TileKind.Wall });
        var variant2 = CreateVariant(tiles: new[] { TileKind.Floor });

        Assert.False(_comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_DifferentConnections_ReturnsFalse()
    {
        var conn1 = new PrefabConnectionPoint(new Point2(0, 0), Direction.North);
        var conn2 = new PrefabConnectionPoint(new Point2(0, 0), Direction.South);

        var variant1 = CreateVariant(connections: new[] { conn1 });
        var variant2 = CreateVariant(connections: new[] { conn2 });

        Assert.False(_comparer.Equals(variant1, variant2));
    }

    [Fact]
    public void Equals_IdenticalVariants_ReturnsTrue()
    {
        var conn = new PrefabConnectionPoint(new Point2(0, 0), Direction.North);
        var variant1 = CreateVariant(
            width: 2,
            height: 1,
            tiles: new[] { TileKind.Wall, TileKind.Floor },
            connections: new[] { conn });

        var variant2 = CreateVariant(
            width: 2,
            height: 1,
            tiles: new[] { TileKind.Wall, TileKind.Floor },
            connections: new[] { conn });

        Assert.True(_comparer.Equals(variant1, variant2));
    }

    [Theory]
    [InlineData((TileKind)999)]
    [InlineData((TileKind)(-1))]
    public void Equals_InvalidTileKindInFirstVariant_ThrowsArgumentOutOfRangeException(TileKind invalidKind)
    {
        var variant1 = CreateVariant(tiles: new[] { invalidKind });
        var variant2 = CreateVariant(tiles: new[] { TileKind.Wall });

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _comparer.Equals(variant1, variant2));
        Assert.Equal("tileKind", ex.ParamName);
    }

    [Theory]
    [InlineData((TileKind)999)]
    [InlineData((TileKind)(-1))]
    public void Equals_InvalidTileKindInSecondVariant_ThrowsArgumentOutOfRangeException(TileKind invalidKind)
    {
        var variant1 = CreateVariant(tiles: new[] { TileKind.Wall });
        var variant2 = CreateVariant(tiles: new[] { invalidKind });

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _comparer.Equals(variant1, variant2));
        Assert.Equal("tileKind", ex.ParamName);
    }

    [Fact]
    public void GetHashCode_NullArgument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _comparer.GetHashCode(null!));
    }

    [Theory]
    [InlineData((TileKind)999)]
    [InlineData((TileKind)(-1))]
    public void GetHashCode_InvalidTileKind_ThrowsArgumentOutOfRangeException(TileKind invalidKind)
    {
        var variant = CreateVariant(tiles: new[] { invalidKind });

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _comparer.GetHashCode(variant));
        Assert.Equal("tileKind", ex.ParamName);
    }

    [Fact]
    public void GetHashCode_EqualVariants_ProduceSameHashCode()
    {
        var conn = new PrefabConnectionPoint(new Point2(0, 0), Direction.North);
        var variant1 = CreateVariant(
            width: 2,
            height: 1,
            tiles: new[] { TileKind.Wall, TileKind.Floor },
            connections: new[] { conn });

        var variant2 = CreateVariant(
            width: 2,
            height: 1,
            tiles: new[] { TileKind.Wall, TileKind.Floor },
            connections: new[] { conn });

        var hash1 = _comparer.GetHashCode(variant1);
        var hash2 = _comparer.GetHashCode(variant2);

        Assert.Equal(hash1, hash2);
    }
}
