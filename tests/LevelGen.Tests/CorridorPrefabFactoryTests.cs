using LevelGen.Internal;
using System;
using Xunit;

namespace LevelGen.Tests;

public class CorridorPrefabFactoryTests
{
    [Fact]
    public void CreateStraightCorridor_ThrowsArgumentOutOfRangeException_WhenFloorLengthIsLessThanOne()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CorridorPrefabFactory.CreateStraightCorridor(0));
        Assert.Equal("floorLength", exception.ParamName);
    }
}
