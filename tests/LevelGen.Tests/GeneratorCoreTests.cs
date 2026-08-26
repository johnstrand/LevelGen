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
}
