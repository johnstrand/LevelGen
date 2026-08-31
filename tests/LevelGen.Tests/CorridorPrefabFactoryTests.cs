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

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void CreateStraightCorridor_ReturnsExpectedPrefabDefinition(int floorLength)
    {
        // Act
        var prefab = CorridorPrefabFactory.CreateStraightCorridor(floorLength);

        // Assert
        Assert.Equal($"Generated corridor ({floorLength})", prefab.Name);
        Assert.Equal(floorLength + 2, prefab.Width);
        Assert.Equal(3, prefab.Height);

        // Check connectors at ends
        Assert.Equal(TileKind.Connector, prefab[0, 1]);
        Assert.Equal(TileKind.Connector, prefab[floorLength + 1, 1]);

        // Check floor tiles in middle
        for (var x = 1; x <= floorLength; x++)
        {
            Assert.Equal(TileKind.Floor, prefab[x, 1]);
        }

        // Check top and bottom wall rows
        for (var x = 0; x < prefab.Width; x++)
        {
            Assert.Equal(TileKind.Wall, prefab[x, 0]);
            Assert.Equal(TileKind.Wall, prefab[x, 2]);
        }
    }

    [Fact]
    public void CreateElbowCorridor_ReturnsExpectedPrefabDefinition()
    {
        // Act
        var prefab = CorridorPrefabFactory.CreateElbowCorridor();

        // Assert
        Assert.Equal("Generated corridor elbow", prefab.Name);
        Assert.Equal(4, prefab.Width);
        Assert.Equal(4, prefab.Height);

        var expectedTiles = new TileKind[,]
        {
            { TileKind.Wall,      TileKind.Wall,      TileKind.Connector, TileKind.Wall },
            { TileKind.Wall,      TileKind.Floor,     TileKind.Floor,     TileKind.Wall },
            { TileKind.Connector, TileKind.Floor,     TileKind.Floor,     TileKind.Wall },
            { TileKind.Wall,      TileKind.Wall,      TileKind.Wall,      TileKind.Wall }
        };

        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                Assert.Equal(expectedTiles[y, x], prefab[x, y]);
            }
        }
    }

    [Fact]
    public void CreateGeneratedCorridors_ThrowsArgumentOutOfRangeException_WhenMaxCorridorLengthIsLessThanOne()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CorridorPrefabFactory.CreateGeneratedCorridors(0));
        Assert.Equal("maxCorridorLength", exception.ParamName);
    }

    [Fact]
    public void CreateGeneratedCorridors_ReturnsCorridorsIncludingElbow()
    {
        // Act
        var corridors = CorridorPrefabFactory.CreateGeneratedCorridors(3);

        // Assert: 3 straight corridors + 1 elbow corridor = 4 corridors
        Assert.Equal(4, corridors.Count);
        Assert.Equal("Generated corridor (1)", corridors[0].Name);
        Assert.Equal("Generated corridor (2)", corridors[1].Name);
        Assert.Equal("Generated corridor (3)", corridors[2].Name);
        Assert.Equal("Generated corridor elbow", corridors[3].Name);
    }
}
