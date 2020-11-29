using System.Numerics;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class BenchmarkVectorNumerics
    {
        [Benchmark]
        public Vector2<int> GenericInt2()
        {
            Vector2<int> a = new Vector2<int>(9);
            Vector2<int> b = new Vector2<int>(9);
            return a * b;
        }

        [Benchmark]
        public Vector3<int> GenericInt3()
        {
            Vector3<int> a = new Vector3<int>(9);
            Vector3<int> b = new Vector3<int>(9);
            return a * b;
        }

        [Benchmark]
        public Vector2<float> GenericFloat2()
        {
            Vector2<float> a = new Vector2<float>(7f);
            Vector2<float> b = new Vector2<float>(7f);
            return a * b;
        }
    }
}
