using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LevelGen.Tests;

public sealed class PrefabVariantTests
{
    private static PrefabDefinition CreateTestPrefab()
    {
        var tiles = new TileKind[] { TileKind.Wall };
        return new PrefabDefinition("TestPrefab", 1, 1, tiles);
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new PrefabVariant(null!));
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullSource_ThrowsArgumentNullException()
    {
        var options = new PrefabVariantOptions
        {
            Source = null!,
            Transform = PrefabTransform.Identity,
            Width = 1,
            Height = 1,
            Tiles = [],
            Connections = [],
            Doodads = [],
        };

        var exception = Assert.Throws<ArgumentNullException>(() => new PrefabVariant(options));
        Assert.Equal("Source", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullTiles_ThrowsArgumentNullException()
    {
        var options = new PrefabVariantOptions
        {
            Source = CreateTestPrefab(),
            Transform = PrefabTransform.Identity,
            Width = 1,
            Height = 1,
            Tiles = null!,
            Connections = [],
            Doodads = [],
        };

        var exception = Assert.Throws<ArgumentNullException>(() => new PrefabVariant(options));
        Assert.Equal("Tiles", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullConnections_ThrowsArgumentNullException()
    {
        var options = new PrefabVariantOptions
        {
            Source = CreateTestPrefab(),
            Transform = PrefabTransform.Identity,
            Width = 1,
            Height = 1,
            Tiles = [],
            Connections = null!,
            Doodads = [],
        };

        var exception = Assert.Throws<ArgumentNullException>(() => new PrefabVariant(options));
        Assert.Equal("Connections", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullDoodads_ThrowsArgumentNullException()
    {
        var options = new PrefabVariantOptions
        {
            Source = CreateTestPrefab(),
            Transform = PrefabTransform.Identity,
            Width = 1,
            Height = 1,
            Tiles = [],
            Connections = [],
            Doodads = null!,
        };

        var exception = Assert.Throws<ArgumentNullException>(() => new PrefabVariant(options));
        Assert.Equal("Doodads", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidArguments_InitializesProperties()
    {
        var source = CreateTestPrefab();
        var transform = new PrefabTransform(1, true);
        var width = 2;
        var height = 3;
        var tiles = new[] { TileKind.Wall, TileKind.Floor };
        var connections = new[] { new PrefabConnectionPoint(new Point2(0, 0), Direction.North) };
        var doodads = new[] { new PrefabDoodad(new Point2(1, 1), 'x') };

        var variant = new PrefabVariantBuilder()
            .WithSource(source)
            .WithTransform(transform)
            .WithWidth(width)
            .WithHeight(height)
            .WithTiles(tiles)
            .WithConnections(connections)
            .WithDoodads(doodads)
            .Build();

        Assert.Same(source, variant.Source);
        Assert.Equal(transform, variant.Transform);
        Assert.Equal(width, variant.Width);
        Assert.Equal(height, variant.Height);

        Assert.Equal(tiles, variant.Tiles);
        Assert.Equal(connections, variant.Connections);
        Assert.Equal(doodads, variant.Doodads);

        Assert.NotNull(variant.LocalConnections);
        Assert.Single(variant.LocalConnections);
        Assert.True(variant.LocalConnections.ContainsKey(new Point2(0, 0)));
        Assert.Equal(connections[0], variant.LocalConnections[new Point2(0, 0)]);
    }

    [Fact]
    public void Constructor_DuplicateConnectionPositions_ThrowsArgumentException()
    {
        var connections = new[]
        {
            new PrefabConnectionPoint(new Point2(0, 0), Direction.North),
            new PrefabConnectionPoint(new Point2(0, 0), Direction.South),
        };

        Assert.Throws<ArgumentException>(() =>
            new PrefabVariantBuilder()
                .WithSource(CreateTestPrefab())
                .WithTransform(PrefabTransform.Identity)
                .WithDimensions(1, 1)
                .WithTiles([TileKind.Wall])
                .WithConnections(connections)
                .WithDoodads([])
                .Build());
    }

    [Fact]
    public void Constructor_InputCollectionsAreCopied_ModificationsToInputDoNotAffectVariant()
    {
        var tiles = new[] { TileKind.Wall, TileKind.Floor };
        var connections = new[] { new PrefabConnectionPoint(new Point2(0, 0), Direction.North) };
        var doodads = new[] { new PrefabDoodad(new Point2(1, 1), 'x') };

        var variant = new PrefabVariantBuilder()
            .WithSource(CreateTestPrefab())
            .WithTransform(PrefabTransform.Identity)
            .WithDimensions(2, 1)
            .WithTiles(tiles)
            .WithConnections(connections)
            .WithDoodads(doodads)
            .Build();

        tiles[0] = TileKind.Empty;
        connections[0] = new PrefabConnectionPoint(new Point2(0, 0), Direction.East);
        doodads[0] = new PrefabDoodad(new Point2(1, 1), 'y');

        Assert.Equal(TileKind.Wall, variant.Tiles[0]);
        Assert.Equal(Direction.North, variant.Connections[0].Facing);
        Assert.Equal('x', variant.Doodads[0].Marker);
    }

    [Fact]
    public void LocalConnections_NonExistentPosition_ReturnsFalse()
    {
        var connections = new[] { new PrefabConnectionPoint(new Point2(0, 0), Direction.North) };
        var variant = new PrefabVariantBuilder()
            .WithSource(CreateTestPrefab())
            .WithTransform(PrefabTransform.Identity)
            .WithDimensions(1, 1)
            .WithTiles([TileKind.Wall])
            .WithConnections(connections)
            .WithDoodads([])
            .Build();

        Assert.False(variant.LocalConnections.ContainsKey(new Point2(99, 99)));
        Assert.False(variant.LocalConnections.TryGetValue(new Point2(99, 99), out _));
    }
}
