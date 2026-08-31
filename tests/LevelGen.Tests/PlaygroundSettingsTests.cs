using LevelGen.Playground;

namespace LevelGen.Tests;

public sealed class PlaygroundSettingsTests
{
    [Fact]
    public void Parse_WithEmptyArgs_ReturnsDefaultSettings()
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
    public void Parse_WithAllOptionsProvided_ReturnsExpectedSettings()
    {
        string[] args = [
            "--seed", "12345",
            "--blocks", "path/to/blocks.txt",
            "--max-prefabs", "12",
            "--max-corridor-length", "15",
            "--no-loops",
            "--no-corridors",
            "--no-mirror",
            "--once"
        ];

        var settings = PlaygroundSettings.Parse(args);

        Assert.Equal(12345, settings.Seed);
        Assert.Equal("path/to/blocks.txt", settings.BlocksPath);
        Assert.Equal(12, settings.MaxPrefabCount);
        Assert.Equal(15, settings.MaxCorridorLength);
        Assert.False(settings.AllowLoops);
        Assert.False(settings.AllowGeneratedCorridors);
        Assert.False(settings.AllowMirrorTransforms);
        Assert.True(settings.RunOnce);
    }

    [Fact]
    public void Parse_WithUnknownArgument_ThrowsArgumentException()
    {
        string[] args = ["--unknown-option"];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Contains("Unknown argument '--unknown-option'", ex.Message);
    }

    [Theory]
    [InlineData("--seed")]
    [InlineData("--blocks")]
    [InlineData("--max-prefabs")]
    [InlineData("--max-corridor-length")]
    public void Parse_WithMissingOptionValue_ThrowsArgumentException(string option)
    {
        string[] args = [option];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Contains($"Option {option} requires a value", ex.Message);
    }

    [Theory]
    [InlineData("--seed", "not-an-int")]
    [InlineData("--max-prefabs", "abc")]
    [InlineData("--max-corridor-length", "12.34")]
    public void Parse_WithInvalidIntegerValue_ThrowsArgumentException(string option, string invalidValue)
    {
        string[] args = [option, invalidValue];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Contains($"Option {option} requires an integer value", ex.Message);
    }
}
