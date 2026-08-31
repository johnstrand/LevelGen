using LevelGen.Playground;

namespace LevelGen.Tests;

public sealed class PlaygroundSettingsTests
{
    [Theory]
    [InlineData("--seed")]
    [InlineData("--max-prefabs")]
    [InlineData("--max-corridor-length")]
    public void Parse_ThrowsArgumentException_WhenOptionValueIsNotAnInteger(string optionName)
    {
        string[] args = [optionName, "invalid-int"];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Equal($"Option {optionName} requires an integer value.", ex.Message);
    }

    [Theory]
    [InlineData("--seed")]
    [InlineData("--blocks")]
    [InlineData("--max-prefabs")]
    [InlineData("--max-corridor-length")]
    public void Parse_ThrowsArgumentException_WhenOptionValueIsMissing(string optionName)
    {
        string[] args = [optionName];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Equal($"Option {optionName} requires a value.", ex.Message);
    }

    [Fact]
    public void Parse_ThrowsArgumentException_WhenUnknownArgumentIsProvided()
    {
        string[] args = ["--unknown-arg"];

        var ex = Assert.Throws<ArgumentException>(() => PlaygroundSettings.Parse(args));

        Assert.Equal("Unknown argument '--unknown-arg'. Use --help to see supported options.", ex.Message);
    }

    [Fact]
    public void Parse_ParsesValidArgumentsSuccessfully()
    {
        string[] args =
        [
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
    public void Parse_ReturnsDefaults_WhenNoArgumentsProvided()
    {
        string[] args = [];

        var settings = PlaygroundSettings.Parse(args);

        Assert.Null(settings.Seed);
        Assert.Null(settings.BlocksPath);
        Assert.Equal(6, settings.MaxPrefabCount);
        Assert.Equal(8, settings.MaxCorridorLength);
        Assert.True(settings.AllowLoops);
        Assert.True(settings.AllowGeneratedCorridors);
        Assert.True(settings.AllowMirrorTransforms);
        Assert.False(settings.RunOnce);
    }
}
