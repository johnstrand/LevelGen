namespace LevelGen.Tests;

public sealed class GenerationOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new GenerationOptions();

        Assert.Equal(0, options.Seed);
        Assert.Null(options.TargetWalkableTileCount);
        Assert.Equal(6, options.MaxPrefabCount);
        Assert.Null(options.MinWidth);
        Assert.Null(options.MaxWidth);
        Assert.Null(options.MinHeight);
        Assert.Null(options.MaxHeight);
        Assert.True(options.AllowLoops);
        Assert.True(options.AllowMirrorTransforms);
        Assert.True(options.AllowGeneratedCorridors);
        Assert.Equal(8, options.MaxCorridorLength);
    }

    [Fact]
    public void CustomValues_CanBeInitialized()
    {
        var options = new GenerationOptions
        {
            Seed = 42,
            TargetWalkableTileCount = 100,
            MaxPrefabCount = 10,
            MinWidth = 10,
            MaxWidth = 20,
            MinHeight = 15,
            MaxHeight = 25,
            AllowLoops = false,
            AllowMirrorTransforms = false,
            AllowGeneratedCorridors = false,
            MaxCorridorLength = 4
        };

        Assert.Equal(42, options.Seed);
        Assert.Equal(100, options.TargetWalkableTileCount);
        Assert.Equal(10, options.MaxPrefabCount);
        Assert.Equal(10, options.MinWidth);
        Assert.Equal(20, options.MaxWidth);
        Assert.Equal(15, options.MinHeight);
        Assert.Equal(25, options.MaxHeight);
        Assert.False(options.AllowLoops);
        Assert.False(options.AllowMirrorTransforms);
        Assert.False(options.AllowGeneratedCorridors);
        Assert.Equal(4, options.MaxCorridorLength);
    }
}
