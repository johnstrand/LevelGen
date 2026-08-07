using BenchmarkDotNet.Running;

namespace LevelGen.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GeneratorBenchmark>();
            BenchmarkRunner.Run<ParserBenchmark>();
        }
    }
}
