using System;
using LevelGen.Internal;

namespace LevelGen.Tests;

public sealed class DirectionExtensionsTests
{
    [Fact]
    public void Offset_ThrowsArgumentOutOfRangeException_ForInvalidDirection()
    {
        var invalidDirection = (Direction)999;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => invalidDirection.Offset());

        Assert.Equal("direction", exception.ParamName);
    }
}
