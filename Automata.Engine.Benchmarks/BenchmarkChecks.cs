using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    public class BenchmarkChecks
    {
        public int ValueA;
        public int ValueAA;
        public int? ValueB;

        [GlobalSetup]
        public void Setup()
        {
            ValueA = 111;
            ValueAA = 1112;
            ValueB = null;
        }

        [Benchmark]
        public bool LessThan() => (uint)ValueA < (uint)ValueAA;

        [Benchmark]
        public bool IsNull() => ValueB is null;
    }
}
