using System;
using Xunit;
using LevelGen.Internal;

namespace LevelGen.Tests;

public sealed class PrefabVariantFactoryTests
{
    [Fact]
    public void ExtractConnections_ConnectorWithZeroInwardCandidates_ThrowsInvalidOperationException()
    {
        // A 1x1 prefab with a connector will have all 4 directions as outward candidates.
        // However, looking inward from all these outward candidates will fall out of bounds,
        // resulting in 0 inward candidates.
        var tiles = new[] { TileKind.Connector };
        var prefab = new PrefabDefinition("ZeroInward", 1, 1, tiles);

        var exception = Assert.Throws<InvalidOperationException>(() => PrefabVariantFactory.ExtractConnections(prefab));
        Assert.Equal("Connector at (0, 0) in prefab 'ZeroInward' must expose exactly one outward-facing side.", exception.Message);
    }

    [Fact]
    public void ExtractConnections_ConnectorWithMultipleInwardCandidates_ThrowsInvalidOperationException()
    {
        // A 2x2 prefab with a connector at (0, 0) and floors at (1, 0) and (0, 1).
        // Outward candidates for (0, 0) are Up (0, -1) and Left (-1, 0) since they are out of bounds.
        // Looking inwards:
        // - From Up (opposite is Down), it checks (0, 1) which is a Floor (Walkable).
        // - From Left (opposite is Right), it checks (1, 0) which is a Floor (Walkable).
        // This results in 2 inward candidates.
        var tiles = new[]
        {
            TileKind.Connector, TileKind.Floor,
            TileKind.Floor, TileKind.Empty
        };
        var prefab = new PrefabDefinition("MultipleInward", 2, 2, tiles);

        var exception = Assert.Throws<InvalidOperationException>(() => PrefabVariantFactory.ExtractConnections(prefab));
        Assert.Equal("Connector at (0, 0) in prefab 'MultipleInward' must expose exactly one outward-facing side.", exception.Message);
    }
}
