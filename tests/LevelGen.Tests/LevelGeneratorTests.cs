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
}
