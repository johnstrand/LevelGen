using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LevelGen;
using System.Linq;
using System.Collections.Generic;

namespace LevelGen.Benchmarks
{
    [MemoryDiagnoser]
    public class GeneratorBenchmark
    {
        private PrefabSet? _prefabSet;
        private GenerationOptions? _options;

        [GlobalSetup]
        public void Setup()
        {
            var room = new PrefabDefinition("Room", 3, 3, new[]
            {
                TileKind.Wall, TileKind.Connector, TileKind.Wall,
                TileKind.Connector, TileKind.Floor, TileKind.Connector,
                TileKind.Wall, TileKind.Connector, TileKind.Wall
            }, System.Array.Empty<PrefabDoodad>());

            _prefabSet = new PrefabSet(new[] { room });
            _options = new GenerationOptions
            {
                Seed = 42,
                MaxPrefabCount = 10,
                AllowMirrorTransforms = true,
                AllowGeneratedCorridors = true,
                AllowLoops = true,
                MaxCorridorLength = 3
            };
        }

        [Benchmark]
        public GenerationResult GenerateLevel()
        {
            return LevelGenerator.Generate(_prefabSet!, _options!);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<GeneratorBenchmark>();
        }
    }
}
