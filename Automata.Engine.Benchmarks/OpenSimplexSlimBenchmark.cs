using System.Numerics;
using Automata.Engine.Noise;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    public class OpenSimplexSlimBenchmark
    {
        private Vector3 _Coordinates;
        private int _Seed;

        [GlobalSetup]
        public void Setup()
        {
            _Coordinates = new Vector3(1f, 6f, 100000f);
            _Seed = 1231313213;
        }

        [Benchmark]
        public float ReadOnlyArray() => OpenSimplexSlim.GetSimplex(_Seed, 0.001f, _Coordinates);
    }
}
