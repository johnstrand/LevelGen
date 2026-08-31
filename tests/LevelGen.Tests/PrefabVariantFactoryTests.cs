using System;
using System.Linq;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public sealed class PrefabVariantFactoryTests
{
    [Theory]
    // 0 quarter turns, no mirror (Identity)
    [InlineData(1, 2, 4, 5, 0, false, 1, 2)]
    // 1 quarter turn clockwise, no mirror
    // 0 1 2 3      0 1 2 3 4
    // 0 . . . .      0 . . .
    // 1 . . . .      1 . . x
    // 2 . x . .  ->  2 . . .
    // 3 . . . .      3 . . .
    // 4 . . . .
    // Point (1, 2) in 4x5 grid -> (5-1-2, 1) = (2, 1)  (WAIT: (currentHeight - 1 - y, x) => 5-1-2 = 2, 1 is correct)
    [InlineData(1, 2, 4, 5, 1, false, 2, 1)]
    // 2 quarter turns, no mirror
    // (currentWidth - 1 - x, currentHeight - 1 - y) => (4-1-1, 5-1-2) = (2, 2)
    [InlineData(1, 2, 4, 5, 2, false, 2, 2)]
    // 3 quarter turns, no mirror
    // Rotate 1: (5-1-2, 1) = (2, 1) [Grid is 5x4]
    // Rotate 2: (4-1-1, 5-1-2) = (2, 2) [Grid is 4x5]
    // Rotate 3: (5-1-2, 2) = (2, 2) [Grid is 5x4]
    [InlineData(1, 2, 4, 5, 3, false, 2, 2)]

    // 0 quarter turns, mirror horizontally
    // (currentWidth - 1 - x, y) => (4-1-1, 2) = (2, 2)
    [InlineData(1, 2, 4, 5, 0, true, 2, 2)]
    // 1 quarter turn clockwise, mirror horizontally
    // First mirror: (2, 2). Then rotate 1: (5-1-2, 2) = (2, 2)
    [InlineData(1, 2, 4, 5, 1, true, 2, 2)]
    // 2 quarter turns, mirror horizontally
    // First mirror: (2, 2). Rotate 2: (4-1-2, 5-1-2) = (1, 2)
    [InlineData(1, 2, 4, 5, 2, true, 1, 2)]
    // 3 quarter turns, mirror horizontally
    // First mirror: (2, 2) [Grid is 4x5]
    // Rotate 1: (5-1-2, 2) = (2, 2) [Grid is 5x4]
    // Rotate 2: (4-1-2, 5-1-2) = (1, 2) [Grid is 4x5]
    // Rotate 3: (5-1-2, 1) = (2, 1) [Grid is 5x4]
    [InlineData(1, 2, 4, 5, 3, true, 2, 1)]

    // Test a different point to avoid (2,2) overlaps in assertions
    // Point (0, 0) in 3x2 grid
    [InlineData(0, 0, 3, 2, 0, false, 0, 0)]
    // Rotate 1: (2-1-0, 0) = (1, 0)
    [InlineData(0, 0, 3, 2, 1, false, 1, 0)]
    // Rotate 2: (3-1-0, 2-1-0) = (2, 1)
    [InlineData(0, 0, 3, 2, 2, false, 2, 1)]
    // Rotate 3: (0, 3-1-0) = (0, 2)
    [InlineData(0, 0, 3, 2, 3, false, 0, 2)]

    // Mirror: (3-1-0, 0) = (2, 0)
    [InlineData(0, 0, 3, 2, 0, true, 2, 0)]
    // Mirror + Rotate 1: (2, 0) -> (2-1-0, 2) = (1, 2)
    [InlineData(0, 0, 3, 2, 1, true, 1, 2)]
    // Mirror + Rotate 2: (2, 0) -> (3-1-2, 2-1-0) = (0, 1)
    [InlineData(0, 0, 3, 2, 2, true, 0, 1)]
    // Mirror + Rotate 3: (2, 0) -> (0, 3-1-2) = (0, 0)
    [InlineData(0, 0, 3, 2, 3, true, 0, 0)]
    public void TransformPoint_ReturnsExpectedCoordinates(
        int x, int y,
        int width, int height,
        int quarterTurns, bool mirror,
        int expectedX, int expectedY)
    {
        var point = new Point2(x, y);
        var transform = new PrefabTransform(quarterTurns, mirror);

        var result = PrefabVariantFactory.TransformPoint(point, width, height, transform);

        Assert.Equal(new Point2(expectedX, expectedY), result);
    }

    [Theory]
    // Identity (0 turns, no mirror)
    [InlineData(Direction.North, 0, false, Direction.North)]
    [InlineData(Direction.East, 0, false, Direction.East)]
    [InlineData(Direction.South, 0, false, Direction.South)]
    [InlineData(Direction.West, 0, false, Direction.West)]

    // Rotations without mirror
    // 1 turn clockwise
    [InlineData(Direction.North, 1, false, Direction.East)]
    [InlineData(Direction.East, 1, false, Direction.South)]
    [InlineData(Direction.South, 1, false, Direction.West)]
    [InlineData(Direction.West, 1, false, Direction.North)]
    // 2 turns clockwise
    [InlineData(Direction.North, 2, false, Direction.South)]
    [InlineData(Direction.East, 2, false, Direction.West)]
    [InlineData(Direction.South, 2, false, Direction.North)]
    [InlineData(Direction.West, 2, false, Direction.East)]
    // 3 turns clockwise
    [InlineData(Direction.North, 3, false, Direction.West)]
    [InlineData(Direction.East, 3, false, Direction.North)]
    [InlineData(Direction.South, 3, false, Direction.East)]
    [InlineData(Direction.West, 3, false, Direction.South)]

    // Horizontal mirror only (0 turns)
    // Mirror flips East <-> West, North/South remain unchanged
    [InlineData(Direction.North, 0, true, Direction.North)]
    [InlineData(Direction.East, 0, true, Direction.West)]
    [InlineData(Direction.South, 0, true, Direction.South)]
    [InlineData(Direction.West, 0, true, Direction.East)]

    // Horizontal mirror + rotations
    // Mirror then 1 turn clockwise:
    // North -> Mirror(North)=North -> Rotate1=East
    [InlineData(Direction.North, 1, true, Direction.East)]
    // East -> Mirror(East)=West -> Rotate1=North
    [InlineData(Direction.East, 1, true, Direction.North)]
    // South -> Mirror(South)=South -> Rotate1=West
    [InlineData(Direction.South, 1, true, Direction.West)]
    // West -> Mirror(West)=East -> Rotate1=South
    [InlineData(Direction.West, 1, true, Direction.South)]

    // Mirror then 2 turns clockwise:
    // North -> Mirror(North)=North -> Rotate2=South
    [InlineData(Direction.North, 2, true, Direction.South)]
    // East -> Mirror(East)=West -> Rotate2=East
    [InlineData(Direction.East, 2, true, Direction.East)]
    // South -> Mirror(South)=South -> Rotate2=North
    [InlineData(Direction.South, 2, true, Direction.North)]
    // West -> Mirror(West)=East -> Rotate2=West
    [InlineData(Direction.West, 2, true, Direction.West)]

    // Mirror then 3 turns clockwise:
    // North -> Mirror(North)=North -> Rotate3=West
    [InlineData(Direction.North, 3, true, Direction.West)]
    // East -> Mirror(East)=West -> Rotate3=South
    [InlineData(Direction.East, 3, true, Direction.South)]
    // South -> Mirror(South)=South -> Rotate3=East
    [InlineData(Direction.South, 3, true, Direction.East)]
    // West -> Mirror(West)=East -> Rotate3=North
    [InlineData(Direction.West, 3, true, Direction.North)]
    public void TransformDirection_ReturnsExpectedDirection(
        Direction initialDirection,
        int quarterTurns,
        bool mirror,
        Direction expectedDirection)
    {
        var transform = new PrefabTransform(quarterTurns, mirror);

        var result = PrefabVariantFactory.TransformDirection(initialDirection, transform);

        Assert.Equal(expectedDirection, result);
    }

    [Fact]
    public void TryInferConnectorFacing_NoOutwardCandidates_ReturnsFalseAndDefaultFacing()
    {
        // 3x3 room with connector in center (1, 1), surrounded by Wall tiles on all 4 sides.
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Wall,      TileKind.Wall,
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
            TileKind.Wall, TileKind.Wall,      TileKind.Wall,
        };
        var prefab = new PrefabDefinition("NoOutward", 3, 3, tiles);

        var result = PrefabVariantFactory.TryInferConnectorFacing(prefab, 1, 1, out var facing);

        Assert.False(result);
        Assert.Equal(default(Direction), facing);
    }

    [Theory]
    [InlineData(1, 0, Direction.North)]
    [InlineData(1, 2, Direction.South)]
    [InlineData(2, 1, Direction.East)]
    [InlineData(0, 1, Direction.West)]
    public void TryInferConnectorFacing_SingleOutwardCandidate_ReturnsTrueAndCorrectFacing(int x, int y, Direction expectedFacing)
    {
        // 3x3 prefab with walls everywhere, except connector at specified position facing out of bounds.
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
            TileKind.Connector, TileKind.Floor, TileKind.Connector,
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
        };
        // Replace non-target connectors with walls to ensure target connector has exactly one outward candidate
        for (var i = 0; i < tiles.Length; i++)
        {
            var tx = i % 3;
            var ty = i / 3;
            if (tx == x && ty == y)
            {
                tiles[i] = TileKind.Connector;
            }
            else
            {
                tiles[i] = TileKind.Wall;
            }
        }
        var prefab = new PrefabDefinition("SingleOutwardBoundary", 3, 3, tiles);

        var result = PrefabVariantFactory.TryInferConnectorFacing(prefab, x, y, out var facing);

        Assert.True(result);
        Assert.Equal(expectedFacing, facing);
    }

    [Fact]
    public void TryInferConnectorFacing_SingleOutwardCandidate_AdjacentEmptyTile_ReturnsTrueAndCorrectFacing()
    {
        // 3x3 room with connector at center (1, 1). Surrounding tiles are Wall except East (2, 1) which is Empty.
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Wall,      TileKind.Wall,
            TileKind.Wall, TileKind.Connector, TileKind.Empty,
            TileKind.Wall, TileKind.Wall,      TileKind.Wall,
        };
        var prefab = new PrefabDefinition("SingleOutwardEmpty", 3, 3, tiles);

        var result = PrefabVariantFactory.TryInferConnectorFacing(prefab, 1, 1, out var facing);

        Assert.True(result);
        Assert.Equal(Direction.East, facing);
    }

    [Fact]
    public void TryInferConnectorFacing_MultipleOutwardCandidates_SingleInwardCandidate_ReturnsTrueAndCorrectFacing()
    {
        // 2x2 with connector at corner (0, 0).
        // Outward candidates: North and West (out of bounds).
        // Opposite of North is South (0, 1) -> Floor (walkable).
        // Opposite of West is East (1, 0) -> Wall (not walkable).
        var tiles = new[]
        {
            TileKind.Connector, TileKind.Wall,
            TileKind.Floor,     TileKind.Wall,
        };
        var prefab = new PrefabDefinition("MultipleOutwardSingleInward", 2, 2, tiles);

        var result = PrefabVariantFactory.TryInferConnectorFacing(prefab, 0, 0, out var facing);

        Assert.True(result);
        Assert.Equal(Direction.North, facing);
    }

    [Fact]
    public void TryInferConnectorFacing_MultipleOutwardCandidates_ZeroInwardCandidates_ThrowsInvalidOperationException()
    {
        var tiles = new[] { TileKind.Connector };
        var prefab = new PrefabDefinition("ZeroInward", 1, 1, tiles);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PrefabVariantFactory.TryInferConnectorFacing(prefab, 0, 0, out _));

        Assert.Equal("Connector at (0, 0) in prefab 'ZeroInward' must expose exactly one outward-facing side.", exception.Message);
    }

    [Fact]
    public void TryInferConnectorFacing_MultipleOutwardCandidates_MultipleInwardCandidates_ThrowsInvalidOperationException()
    {
        var tiles = new[]
        {
            TileKind.Connector, TileKind.Floor,
            TileKind.Floor,     TileKind.Empty
        };
        var prefab = new PrefabDefinition("MultipleInward", 2, 2, tiles);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PrefabVariantFactory.TryInferConnectorFacing(prefab, 0, 0, out _));

        Assert.Equal("Connector at (0, 0) in prefab 'MultipleInward' must expose exactly one outward-facing side.", exception.Message);
    }

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

    [Fact]
    public void CreateVariants_NullPrefab_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => PrefabVariantFactory.CreateVariants(null!, allowMirror: false));
        Assert.Equal("prefab", exception.ParamName);
    }

    [Fact]
    public void CreateVariants_AsymmetricPrefab_NoMirror_GeneratesFourRotationalVariants()
    {
        // Asymmetric 2x3 prefab with connector at (1, 0) and a doodad at (0, 1)
        // W C
        // D F
        // W W
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Connector,
            TileKind.Floor, TileKind.Floor,
            TileKind.Wall, TileKind.Wall,
        };
        var doodads = new[] { new PrefabDoodad(new Point2(0, 1), 'C') };
        var prefab = new PrefabDefinition("Asymmetric", 2, 3, tiles, doodads);

        var variants = PrefabVariantFactory.CreateVariants(prefab, allowMirror: false);

        Assert.Equal(4, variants.Count);
        Assert.All(variants, v => Assert.False(v.Transform.MirrorHorizontally));
        Assert.Contains(variants, v => v.Transform.QuarterTurnsClockwise == 0 && v.Width == 2 && v.Height == 3);
        Assert.Contains(variants, v => v.Transform.QuarterTurnsClockwise == 1 && v.Width == 3 && v.Height == 2);
        Assert.Contains(variants, v => v.Transform.QuarterTurnsClockwise == 2 && v.Width == 2 && v.Height == 3);
        Assert.Contains(variants, v => v.Transform.QuarterTurnsClockwise == 3 && v.Width == 3 && v.Height == 2);
    }

    [Fact]
    public void CreateVariants_AsymmetricPrefab_AllowMirror_GeneratesEightVariants()
    {
        // Asymmetric 2x3 prefab
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Connector,
            TileKind.Floor, TileKind.Floor,
            TileKind.Wall, TileKind.Wall,
        };
        var doodads = new[] { new PrefabDoodad(new Point2(0, 1), 'C') };
        var prefab = new PrefabDefinition("AsymmetricMirror", 2, 3, tiles, doodads);

        var variants = PrefabVariantFactory.CreateVariants(prefab, allowMirror: true);

        Assert.Equal(8, variants.Count);
    }

    [Fact]
    public void CreateVariants_SymmetricPrefab_DeduplicatesIdenticalVariants()
    {
        // Fully symmetric 3x3 square room with identical connectors on all 4 sides
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
            TileKind.Connector, TileKind.Floor, TileKind.Connector,
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
        };
        var prefab = new PrefabDefinition("Symmetric", 3, 3, tiles);

        var variants = PrefabVariantFactory.CreateVariants(prefab, allowMirror: true);

        // Since all 4 rotations and mirrored versions result in identical tile/connection keys,
        // deduplication should yield exactly 1 variant.
        Assert.Single(variants);
    }

    [Fact]
    public void CreateVariants_UnextractedConnector_ReplacedWithFloorInVariantTiles()
    {
        // 3x3 room with an inner connector surrounded by walls (will not be extracted as connection point)
        var tiles = new[]
        {
            TileKind.Wall, TileKind.Wall, TileKind.Wall,
            TileKind.Wall, TileKind.Connector, TileKind.Wall,
            TileKind.Wall, TileKind.Wall, TileKind.Wall,
        };
        var prefab = new PrefabDefinition("InnerConnectorRoom", 3, 3, tiles);

        var variants = PrefabVariantFactory.CreateVariants(prefab, allowMirror: false);

        var variant = Assert.Single(variants); // Fully symmetric -> 1 variant
        Assert.Empty(variant.Connections);
        // Center tile at index 4 (1, 1) should be replaced with TileKind.Floor
        Assert.Equal(TileKind.Floor, variant.Tiles[(1 * variant.Width) + 1]);
    }
}
