using System;
using System.Linq;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public sealed class PrefabVariantFactoryTests
{
    [Fact]
    public void ExtractConnections_NullPrefab_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => PrefabVariantFactory.ExtractConnections(null!));
        Assert.Equal("prefab", exception.ParamName);
    }

    [Fact]
    public void ExtractConnections_NoConnectors_ReturnsEmptyList()
    {
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Wall, TileKind.Wall,
            TileKind.Wall, TileKind.Floor, TileKind.Wall,
            TileKind.Wall, TileKind.Wall, TileKind.Wall,
        };
        var prefab = new PrefabDefinition("NoConnectors", 3, 3, tiles);

        var result = PrefabVariantFactory.ExtractConnections(prefab);

        Assert.Empty(result);
    }

    [Fact]
    public void ExtractConnections_ValidConnectors_ExtractsCorrectly()
    {
        // 3x3 with connectors on North (1,0) and South (1,2)
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
            TileKind.Wall, TileKind.Floor,     TileKind.Wall,
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
        };
        var prefab = new PrefabDefinition("ValidConnectors", 3, 3, tiles);

        var result = PrefabVariantFactory.ExtractConnections(prefab);

        Assert.Equal(2, result.Count);

        var northConnection = Assert.Single(result, c => c.Facing == Direction.North);
        Assert.Equal(new Point2(1, 0), northConnection.Position);

        var southConnection = Assert.Single(result, c => c.Facing == Direction.South);
        Assert.Equal(new Point2(1, 2), southConnection.Position);
    }

    [Fact]
    public void ExtractConnections_ConnectorNotOutward_SkipsConnection()
    {
        // 3x3 where the connector is in the middle surrounded by walls, so it's never "outward"
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Wall,      TileKind.Wall,
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
            TileKind.Wall, TileKind.Wall,      TileKind.Wall,
        };
        var prefab = new PrefabDefinition("InnerConnector", 3, 3, tiles);

        var result = PrefabVariantFactory.ExtractConnections(prefab);

        Assert.Empty(result);
    }

    [Fact]
    public void ExtractConnections_CornerConnectorWithOneInward_InfersCorrectly()
    {
        // 2x2 with a connector at (1,0). It's at a corner so North and East are both outward.
        // The inward side should be (0,0) (West) or (1,1) (South).
        // Let's make (1,1) a Floor (walkable), and (0,0) a Wall (not walkable).
        // Then inward is South, so outward must be North.
        var tiles = new[]
        {
            TileKind.Wall,  TileKind.Connector,
            TileKind.Empty, TileKind.Floor,
        };
        // Wait, opposite of outward is inward.
        // If facing is North (0,-1), opposite is South (0,1). (1,0) + (0,1) = (1,1) is Floor.
        // If facing is East (1,0), opposite is West (-1,0). (1,0) + (-1,0) = (0,0) is Wall.
        // Therefore, only North has a walkable opposite. Facing should be North.
        var prefab = new PrefabDefinition("CornerConnector", 2, 2, tiles);

        var result = PrefabVariantFactory.ExtractConnections(prefab);

        var connection = Assert.Single(result);
        Assert.Equal(new Point2(1, 0), connection.Position);
        Assert.Equal(Direction.North, connection.Facing);
    }

    [Fact]
    public void ExtractConnections_AmbiguousConnector_ThrowsInvalidOperationException()
    {
        // 2x2 with a connector at (1,0).
        // Let's make both (0,0) [West] and (1,1) [South] Floor (walkable).
        // Then it has two valid outward directions (North and East) with valid inward walkable tiles.
        var tiles = new[]
        {
            TileKind.Floor, TileKind.Connector,
            TileKind.Empty, TileKind.Floor,
        };
        var prefab = new PrefabDefinition("AmbiguousConnector", 2, 2, tiles);

        var exception = Assert.Throws<InvalidOperationException>(() => PrefabVariantFactory.ExtractConnections(prefab));
        Assert.Contains("Connector at (1, 0) in prefab 'AmbiguousConnector' must expose exactly one outward-facing side", exception.Message);
    }

    [Fact]
    public void ExtractConnections_CornerConnectorWithNoInward_ThrowsInvalidOperationException()
    {
        // 2x2 with a connector at (1,0).
        // Both (0,0) and (1,1) are walls (not walkable).
        var tiles = new[]
        {
            TileKind.Wall,  TileKind.Connector,
            TileKind.Empty, TileKind.Wall,
        };
        var prefab = new PrefabDefinition("NoInwardConnector", 2, 2, tiles);

        var exception = Assert.Throws<InvalidOperationException>(() => PrefabVariantFactory.ExtractConnections(prefab));
        Assert.Contains("must expose exactly one outward-facing side", exception.Message);
    }
}
