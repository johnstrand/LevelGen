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
    public void Constructor_NullSource_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PrefabVariant(
                source: null!,
                transform: PrefabTransform.Identity,
                width: 1,
                height: 1,
                tiles: [],
                connections: [],
                doodads: []));

        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullTiles_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PrefabVariant(
                source: CreateTestPrefab(),
                transform: PrefabTransform.Identity,
                width: 1,
                height: 1,
                tiles: null!,
                connections: [],
                doodads: []));

        Assert.Equal("tiles", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullConnections_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PrefabVariant(
                source: CreateTestPrefab(),
                transform: PrefabTransform.Identity,
                width: 1,
                height: 1,
                tiles: [],
                connections: null!,
                doodads: []));

        Assert.Equal("connections", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullDoodads_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PrefabVariant(
                source: CreateTestPrefab(),
                transform: PrefabTransform.Identity,
                width: 1,
                height: 1,
                tiles: [],
                connections: [],
                doodads: null!));

        Assert.Equal("doodads", exception.ParamName);
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

        var variant = new PrefabVariant(
            source,
            transform,
            width,
            height,
            tiles,
            connections,
            doodads);

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
            new PrefabVariant(
                source: CreateTestPrefab(),
                transform: PrefabTransform.Identity,
                width: 1,
                height: 1,
                tiles: [TileKind.Wall],
                connections: connections,
                doodads: []));
    }

    [Fact]
    public void Constructor_InputCollectionsAreCopied_ModificationsToInputDoNotAffectVariant()
    {
        var tiles = new[] { TileKind.Wall, TileKind.Floor };
        var connections = new[] { new PrefabConnectionPoint(new Point2(0, 0), Direction.North) };
        var doodads = new[] { new PrefabDoodad(new Point2(1, 1), 'x') };

        var variant = new PrefabVariant(
            CreateTestPrefab(),
            PrefabTransform.Identity,
            2,
            1,
            tiles,
            connections,
            doodads);

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
        var variant = new PrefabVariant(
            CreateTestPrefab(),
            PrefabTransform.Identity,
            1,
            1,
            [TileKind.Wall],
            connections,
            []);

        Assert.False(variant.LocalConnections.ContainsKey(new Point2(99, 99)));
        Assert.False(variant.LocalConnections.TryGetValue(new Point2(99, 99), out _));
    }
}
