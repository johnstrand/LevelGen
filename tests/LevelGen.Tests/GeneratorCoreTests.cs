using LevelGen;
using LevelGen.Blocks;
using LevelGen.Internal;
using Xunit;

namespace LevelGen.Tests;

public class GeneratorCoreTests
{
    [Fact]
    public void AllDirections_ContainsAllDirections()
    {
        // Assert
        Assert.Equal(Enum.GetValues<Direction>(), GeneratorCore.AllDirections);
    }

    [Fact]
    public void Generate_WithMaxWidthAndMaxHeight_ConstrainsDimensions()
    {
        var prefabSet = BlocksPrefabParser.Parse(TestPrefabs.Standard3x3Room);

        var options = new GenerationOptions
        {
            Seed = 42,
            MaxPrefabCount = 4,
            MaxWidth = 6,
            MaxHeight = 6,
            AllowGeneratedCorridors = false
        };

        var result = LevelGenerator.Generate(prefabSet, options);

        Assert.NotNull(result);
        Assert.True(result.Map.Width <= 6, $"Map width {result.Map.Width} exceeded MaxWidth 6");
        Assert.True(result.Map.Height <= 6, $"Map height {result.Map.Height} exceeded MaxHeight 6");
    }

    [Fact]
    public void Generate_WhenImpossibleToSatisfyMinSize_ReturnsBestEffortResultWithoutThrowing()
    {
        var prefabSet = BlocksPrefabParser.Parse(TestPrefabs.SmallRoom1x1);

        var options = new GenerationOptions
        {
            Seed = 1,
            MaxPrefabCount = 1,
            MinWidth = 50,
            MinHeight = 50,
            AllowGeneratedCorridors = false
        };

        var result = LevelGenerator.Generate(prefabSet, options);

        Assert.NotNull(result);
        Assert.Equal(1, result.Map.Width);
        Assert.Equal(1, result.Map.Height);
    }

    [Fact]
    public void LayoutState_Clone_CreatesExactCopyOfAllFields()
    {
        var prefabSet = BlocksPrefabParser.Parse(TestPrefabs.Standard3x3Room);
        var variant = PrefabVariantFactory.CreateVariants(prefabSet[0], false)[0];

        var state = new GeneratorCore.LayoutState
        {
            RoomPlacementCount = 3,
            CorridorPlacementCount = 2
        };
        state.OccupiedTiles[new Point2(1, 2)] = TileKind.Floor;
        state.OpenConnectors[new Point2(3, 4)] = new GeneratorCore.OpenConnector(new Point2(3, 4), Direction.North);
        state.ConnectedConnectorPositions.Add(new Point2(5, 6));
        state.Placements.Add(new GeneratorCore.Placement(variant, new Point2(0, 0), IsCorridor: false));

        var clone = state.Clone();

        Assert.NotSame(state, clone);
        Assert.Equal(state.RoomPlacementCount, clone.RoomPlacementCount);
        Assert.Equal(state.CorridorPlacementCount, clone.CorridorPlacementCount);
        Assert.Equal(state.OccupiedTiles, clone.OccupiedTiles);
        Assert.Equal(state.OpenConnectors, clone.OpenConnectors);
        Assert.Equal(state.ConnectedConnectorPositions, clone.ConnectedConnectorPositions);
        Assert.Equal(state.Placements, clone.Placements);
    }

    [Fact]
    public void LayoutState_Clone_ModificationsToCloneDoNotAffectOriginal()
    {
        var prefabSet = BlocksPrefabParser.Parse(TestPrefabs.Standard3x3Room);
        var variant = PrefabVariantFactory.CreateVariants(prefabSet[0], false)[0];

        var state = new GeneratorCore.LayoutState
        {
            RoomPlacementCount = 3,
            CorridorPlacementCount = 2
        };
        state.OccupiedTiles[new Point2(1, 2)] = TileKind.Floor;
        state.OpenConnectors[new Point2(3, 4)] = new GeneratorCore.OpenConnector(new Point2(3, 4), Direction.North);
        state.ConnectedConnectorPositions.Add(new Point2(5, 6));
        state.Placements.Add(new GeneratorCore.Placement(variant, new Point2(0, 0), IsCorridor: false));

        var clone = state.Clone();

        clone.RoomPlacementCount = 99;
        clone.CorridorPlacementCount = 88;
        clone.OccupiedTiles[new Point2(9, 9)] = TileKind.Wall;
        clone.OccupiedTiles.Remove(new Point2(1, 2));
        clone.OpenConnectors[new Point2(8, 8)] = new GeneratorCore.OpenConnector(new Point2(8, 8), Direction.South);
        clone.ConnectedConnectorPositions.Add(new Point2(7, 7));
        clone.Placements.Clear();

        Assert.Equal(3, state.RoomPlacementCount);
        Assert.Equal(2, state.CorridorPlacementCount);
        Assert.Single(state.OccupiedTiles);
        Assert.True(state.OccupiedTiles.ContainsKey(new Point2(1, 2)));
        Assert.Single(state.OpenConnectors);
        Assert.Single(state.ConnectedConnectorPositions);
        Assert.Single(state.Placements);
    }

    [Fact]
    public void LayoutState_Clone_EmptyState_ReturnsNewEmptyInstance()
    {
        var emptyState = new GeneratorCore.LayoutState();

        var clone = emptyState.Clone();

        Assert.NotSame(emptyState, clone);
        Assert.Equal(0, clone.RoomPlacementCount);
        Assert.Equal(0, clone.CorridorPlacementCount);
        Assert.Empty(clone.OccupiedTiles);
        Assert.Empty(clone.OpenConnectors);
        Assert.Empty(clone.ConnectedConnectorPositions);
        Assert.Empty(clone.Placements);
    }
}
