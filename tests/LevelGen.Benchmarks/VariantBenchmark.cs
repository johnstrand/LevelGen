using BenchmarkDotNet.Attributes;
using LevelGen;
using LevelGen.Internal;
using System;
using System.Collections.Generic;

namespace LevelGen.Benchmarks
{
    [MemoryDiagnoser]
    public class VariantBenchmark
    {
        private PrefabDefinition? _smallPrefab;
        private PrefabDefinition? _largePrefab;

        [GlobalSetup]
        public void Setup()
        {
            // Small 3x3 prefab
            var smallTiles = new TileKind[9];
            Array.Fill(smallTiles, TileKind.Floor);
            smallTiles[0] = TileKind.Wall;
            smallTiles[1] = TileKind.Connector;
            smallTiles[2] = TileKind.Wall;
            smallTiles[3] = TileKind.Connector;
            smallTiles[5] = TileKind.Connector;
            smallTiles[6] = TileKind.Wall;
            smallTiles[7] = TileKind.Connector;
            smallTiles[8] = TileKind.Wall;
            _smallPrefab = new PrefabDefinition("SmallRoom", 3, 3, smallTiles);

            // Large 30x30 prefab with multiple connectors
            const int width = 30;
            const int height = 30;
            var largeTiles = new TileKind[width * height];
            Array.Fill(largeTiles, TileKind.Floor);

            // Set outer walls
            for (var x = 0; x < width; x++)
            {
                largeTiles[x] = TileKind.Wall;
                largeTiles[(height - 1) * width + x] = TileKind.Wall;
            }
            for (var y = 0; y < height; y++)
            {
                largeTiles[y * width] = TileKind.Wall;
                largeTiles[y * width + (width - 1)] = TileKind.Wall;
            }

            // Add connectors along top and bottom walls
            for (var x = 2; x < width - 2; x += 2)
            {
                largeTiles[x] = TileKind.Connector;
                largeTiles[(height - 1) * width + x] = TileKind.Connector;
            }

            _largePrefab = new PrefabDefinition("LargeRoom", width, height, largeTiles);
        }

        [Benchmark]
        public IReadOnlyList<PrefabVariant> CreateVariants_Small()
        {
            return PrefabVariantFactory.CreateVariants(_smallPrefab!, allowMirror: true);
        }

        [Benchmark]
        public IReadOnlyList<PrefabVariant> CreateVariants_Large()
        {
            return PrefabVariantFactory.CreateVariants(_largePrefab!, allowMirror: true);
        }
    }
}
