using LevelGen.Playground;

namespace LevelGen.Tests;

public sealed class PlaygroundSettingsTests
{
    [Fact]
    public void Parse_ThrowsArgumentException_WhenUnknownArgumentIsProvided()
    {
        string[] args = ["--unknown-arg"];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Contains("Unknown argument '--unknown-arg'", ex.Message);
    }

    [Fact]
    public void Parse_ReturnsDefaultValues_WhenArgsAreEmpty()
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
    public void Parse_ParsesValidOptions()
    {
        string[] args = [
            "--seed", "12345",
            "--blocks", "path/to/blocks.txt",
            "--max-prefabs", "10",
            "--max-corridor-length", "12",
            "--no-loops",
            "--no-corridors",
            "--no-mirror",
            "--once"
        ];

        var settings = PlaygroundSettings.Parse(args);

        Assert.Equal(12345, settings.Seed);
        Assert.Equal("path/to/blocks.txt", settings.BlocksPath);
        Assert.Equal(10, settings.MaxPrefabCount);
        Assert.Equal(12, settings.MaxCorridorLength);
        Assert.False(settings.AllowLoops);
        Assert.False(settings.AllowGeneratedCorridors);
        Assert.False(settings.AllowMirrorTransforms);
        Assert.True(settings.RunOnce);
    }

    [Theory]
    [InlineData("--seed")]
    [InlineData("--blocks")]
    [InlineData("--max-prefabs")]
    [InlineData("--max-corridor-length")]
    public void Parse_ThrowsArgumentException_WhenOptionValueIsMissing(string option)
    {
        string[] args = [option];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Contains($"Option {option} requires a value.", ex.Message);
    }

    [Theory]
    [InlineData("--seed", "not-a-number")]
    [InlineData("--max-prefabs", "abc")]
    [InlineData("--max-corridor-length", "1.5")]
    public void Parse_ThrowsArgumentException_WhenIntegerOptionHasInvalidValue(string option, string value)
    {
        string[] args = [option, value];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Contains($"Option {option} requires an integer value.", ex.Message);
    }
}
