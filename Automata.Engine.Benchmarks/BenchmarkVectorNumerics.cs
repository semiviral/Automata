using System.Numerics;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class BenchmarkVectorNumerics
    {
       // [Benchmark]
        public Vector2<int> Generic2() => new Vector2<int>(9) + new Vector2<int>(9);

        //[Benchmark]
        public Vector3<int> Generic3() => new Vector3<int>(9) + new Vector3<int>(9);

        //[Benchmark]
        public Vector4<uint> Generic4() => new Vector4<uint>(9) - new Vector4<uint>(9);

        [Benchmark]
        public Vector4<float> GenericFloating4() => new Vector4<float>(7f) * new Vector4<float>(7f);
    }
}
