using Automata.Engine.Numerics;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    public class BenchmarkPrimitiveConvert
    {
        //[Benchmark]
        public long Unspecialized() => Primitive<int>.Convert<long>(5);

        [Benchmark]
        public long Specialized() => Primitive<int>.ConvertSepcialized<long>(5);
    }
}
