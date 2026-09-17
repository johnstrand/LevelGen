namespace LevelGen.Tests;

public sealed class GenerationResultTests
{
    [Fact]
    public void Constructor_RejectsNullMap()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GenerationResult(null!, []));
        Assert.Equal("map", exception.ParamName);
    }

    [Fact]
    public void Constructor_RejectsNullPlacements()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GenerationResult(new LevelMap([], 0, 0), null!));
        Assert.Equal("placements", exception.ParamName);
    }

    [Fact]
    public void Empty_ReturnsEmptyMapAndPlacements()
    {
        var emptyResult = GenerationResult.Empty;

        Assert.NotNull(emptyResult);
        Assert.NotNull(emptyResult.Map);
        Assert.Equal(0, emptyResult.Map.Width);
        Assert.Equal(0, emptyResult.Map.Height);
        Assert.Empty(emptyResult.Map.AsLinearTiles());
        Assert.NotNull(emptyResult.Placements);
        Assert.Empty(emptyResult.Placements);
    }
}
