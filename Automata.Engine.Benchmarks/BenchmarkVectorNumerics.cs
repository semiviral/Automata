using System.Numerics;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class BenchmarkVectorNumerics
    {
        //[Benchmark]
        public Vector2<short> Generic2()
        {
            Vector2<short> a = new Vector2<short>(9);
            Vector2<short> b = new Vector2<short>(9);
            return a * b;
        }

        //[Benchmark]
        public Vector3<short> Generic3()
        {
            Vector3<short> a = new Vector3<short>(9);
            Vector3<short> b = new Vector3<short>(9);
            return a * b;
        }

        [Benchmark]
        public Vector4<short> Generic4()
        {
            Vector4<short> a = new Vector4<short>(9);
            Vector4<short> b = new Vector4<short>(9);
            return a * b;
        }

        [Benchmark]
        public Vector4<double> GenericFloating2() => new Vector4<double>(7f) * new Vector4<double>(7f);
    }
}
