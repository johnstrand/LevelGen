using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LevelGen;
using LevelGen.Blocks;

namespace LevelGen.Benchmarks
{
    [MemoryDiagnoser]
    public class ParserBenchmark
    {
        private string? _text;

        [GlobalSetup]
        public void Setup()
        {
            _text = @"> Room1
###
#.#
###

> Room2
#####
#...#
#####

> Room3
###
#.#
#.#
###
";
        }

        [Benchmark]
        public PrefabSet ParsePrefab()
        {
            return BlocksPrefabParser.Parse(_text!);
        }
    }
}
