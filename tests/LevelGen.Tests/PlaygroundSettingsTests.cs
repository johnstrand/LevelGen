using LevelGen.Playground;

namespace LevelGen.Tests;

public sealed class PlaygroundSettingsTests
{
    [Theory]
    [InlineData("--blocks")]
    [InlineData("--seed")]
    [InlineData("--max-prefabs")]
    [InlineData("--max-corridor-length")]
    public void Parse_ThrowsArgumentException_WhenOptionValueIsMissing(string option)
    {
        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse([option]));
        Assert.Contains($"Option {option} requires a value.", ex.Message);
    }

    [Theory]
    [InlineData("--seed")]
    [InlineData("--max-prefabs")]
    [InlineData("--max-corridor-length")]
    public void Parse_ThrowsArgumentException_WhenIntegerOptionIsInvalid(string option)
    {
        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse([option, "not-an-int"]));
        Assert.Contains($"Option {option} requires an integer value.", ex.Message);
    }

    [Fact]
    public void Parse_ThrowsArgumentException_WhenUnknownArgumentGiven()
    {
        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(["--unknown"]));
        Assert.Contains("Unknown argument '--unknown'", ex.Message);
    }

    [Fact]
    public void Parse_ReturnsDefaultSettings_WhenArgsIsEmpty()
    {
        var settings = PlaygroundSettings.Parse([]);

        Assert.Null(settings.Seed);
        Assert.Null(settings.BlocksPath);
        Assert.Equal(6, settings.MaxPrefabCount);
        Assert.True(settings.AllowLoops);
        Assert.True(settings.AllowGeneratedCorridors);
        Assert.True(settings.AllowMirrorTransforms);
        Assert.Equal(8, settings.MaxCorridorLength);
        Assert.False(settings.RunOnce);
    }

    [Fact]
    public void Parse_ParsesAllValidOptionsCorrectly()
    {
        string[] args = [
            "--seed", "12345",
            "--blocks", "custom_blocks.txt",
            "--max-prefabs", "12",
            "--max-corridor-length", "15",
            "--no-loops",
            "--no-corridors",
            "--no-mirror",
            "--once"
        ];

        var settings = PlaygroundSettings.Parse(args);

        Assert.Equal(12345, settings.Seed);
        Assert.Equal("custom_blocks.txt", settings.BlocksPath);
        Assert.Equal(12, settings.MaxPrefabCount);
        Assert.Equal(15, settings.MaxCorridorLength);
        Assert.False(settings.AllowLoops);
        Assert.False(settings.AllowGeneratedCorridors);
        Assert.False(settings.AllowMirrorTransforms);
        Assert.True(settings.RunOnce);
    }
}
