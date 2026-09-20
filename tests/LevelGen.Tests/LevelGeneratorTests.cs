using System;
using LevelGen;
using Xunit;

namespace LevelGen.Tests;

public class LevelGeneratorTests
{
    private static PrefabSet CreateValidPrefabSet()
    {
        var tiles = new[] { TileKind.Floor };
        var prefab = new PrefabDefinition("Room", 1, 1, tiles);
        return new PrefabSet([prefab]);
    }

    [Fact]
    public void Generate_NullPrefabSet_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("prefabSet", () => LevelGenerator.Generate(null!));
    }

    [Fact]
    public void Generate_EmptyPrefabSet_ThrowsArgumentException()
    {
        var emptyPrefabSet = new PrefabSet([]);

        var exception = Assert.Throws<ArgumentException>("prefabSet", () => LevelGenerator.Generate(emptyPrefabSet));
        Assert.Contains("At least one prefab is required to generate a level.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Generate_InvalidMaxPrefabCount_ThrowsArgumentOutOfRangeException(int maxPrefabCount)
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MaxPrefabCount = maxPrefabCount };

        Assert.Throws<ArgumentOutOfRangeException>("MaxPrefabCount", () => LevelGenerator.Generate(prefabSet, options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Generate_InvalidTargetWalkableTileCount_ThrowsArgumentOutOfRangeException(int targetWalkableTileCount)
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { TargetWalkableTileCount = targetWalkableTileCount };

        Assert.Throws<ArgumentOutOfRangeException>("TargetWalkableTileCount", () => LevelGenerator.Generate(prefabSet, options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Generate_InvalidMaxCorridorLength_ThrowsArgumentOutOfRangeException(int maxCorridorLength)
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MaxCorridorLength = maxCorridorLength };

        Assert.Throws<ArgumentOutOfRangeException>("MaxCorridorLength", () => LevelGenerator.Generate(prefabSet, options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Generate_InvalidMinWidth_ThrowsArgumentOutOfRangeException(int minWidth)
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MinWidth = minWidth };

        Assert.Throws<ArgumentOutOfRangeException>("MinWidth", () => LevelGenerator.Generate(prefabSet, options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Generate_InvalidMaxWidth_ThrowsArgumentOutOfRangeException(int maxWidth)
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MaxWidth = maxWidth };

        Assert.Throws<ArgumentOutOfRangeException>("MaxWidth", () => LevelGenerator.Generate(prefabSet, options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Generate_InvalidMinHeight_ThrowsArgumentOutOfRangeException(int minHeight)
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MinHeight = minHeight };

        Assert.Throws<ArgumentOutOfRangeException>("MinHeight", () => LevelGenerator.Generate(prefabSet, options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Generate_InvalidMaxHeight_ThrowsArgumentOutOfRangeException(int maxHeight)
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MaxHeight = maxHeight };

        Assert.Throws<ArgumentOutOfRangeException>("MaxHeight", () => LevelGenerator.Generate(prefabSet, options));
    }

    [Fact]
    public void Generate_MinWidthGreaterThanMaxWidth_ThrowsArgumentException()
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MinWidth = 20, MaxWidth = 10 };

        var ex = Assert.Throws<ArgumentException>("options", () => LevelGenerator.Generate(prefabSet, options));
        Assert.Contains("MinWidth cannot be greater than MaxWidth.", ex.Message);
    }

    [Fact]
    public void Generate_MinHeightGreaterThanMaxHeight_ThrowsArgumentException()
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions { MinHeight = 30, MaxHeight = 15 };

        var ex = Assert.Throws<ArgumentException>("options", () => LevelGenerator.Generate(prefabSet, options));
        Assert.Contains("MinHeight cannot be greater than MaxHeight.", ex.Message);
    }

    [Fact]
    public void Generate_NullOptions_UsesDefaultOptionsAndSucceeds()
    {
        var prefabSet = CreateValidPrefabSet();

        var result = LevelGenerator.Generate(prefabSet, null);

        Assert.NotNull(result);
        Assert.NotNull(result.Map);
    }

    [Fact]
    public void Generate_ValidOptions_Succeeds()
    {
        var prefabSet = CreateValidPrefabSet();
        var options = new GenerationOptions
        {
            Seed = 12345,
            MaxPrefabCount = 5,
            AllowLoops = false
        };

        var result = LevelGenerator.Generate(prefabSet, options);

        Assert.NotNull(result);
        Assert.NotNull(result.Map);
    }

    [Fact]
    public void Generate_WithTargetWalkableTileCount_ReachesTargetWalkableTiles()
    {
        var prefabSet = LevelGen.Blocks.BlocksPrefabParser.Parse(TestPrefabs.Standard3x3Room);
        var options = new GenerationOptions
        {
            Seed = 42,
            TargetWalkableTileCount = 10,
            MaxPrefabCount = null,
            AllowGeneratedCorridors = true
        };

        var result = LevelGenerator.Generate(prefabSet, options);

        Assert.NotNull(result);
        var walkableCount = 0;
        for (var y = 0; y < result.Map.Height; y++)
        {
            for (var x = 0; x < result.Map.Width; x++)
            {
                if (result.Map[x, y] == TileKind.Floor)
                {
                    walkableCount++;
                }
            }
        }

        Assert.True(walkableCount >= 10, $"Expected at least 10 walkable tiles, but got {walkableCount}");
    }

    [Fact]
    public void Generate_WithBothTargetWalkableTileCountAndMaxPrefabCount_RespectsMaxPrefabCountLimit()
    {
        var prefabSet = LevelGen.Blocks.BlocksPrefabParser.Parse(TestPrefabs.Standard3x3Room);
        var options = new GenerationOptions
        {
            Seed = 42,
            TargetWalkableTileCount = 1000,
            MaxPrefabCount = 2,
            AllowGeneratedCorridors = false
        };

        var result = LevelGenerator.Generate(prefabSet, options);

        Assert.NotNull(result);
        var roomPlacements = 0;
        foreach (var placement in result.Placements)
        {
            if (!placement.IsCorridor)
            {
                roomPlacements++;
            }
        }

        Assert.True(roomPlacements <= 2, $"Expected at most 2 room placements, but got {roomPlacements}");
    }
}
