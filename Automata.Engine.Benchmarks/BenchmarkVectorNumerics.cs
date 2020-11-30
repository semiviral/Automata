using System.Numerics;
using System.Runtime.CompilerServices;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using BenchmarkDotNet.Attributes;
using Vector = Automata.Engine.Numerics.Vector;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class BenchmarkVectorNumerics
    {
        private Vector4<float> GenericA;
        private Vector4<float> GenericB;
        private Vector<float> CoreA;
        private Vector<float> CoreB;

        [GlobalSetup]
        public void Setup()
        {
            GenericA = Vector4<float>.One;
            GenericB  = new Vector4<float>(4);

            CoreA = Vector<float>.One;
            CoreB = new Vector<float>(4);
        }

       // [Benchmark]
        public Vector2<int> Generic2() => new Vector2<int>(9) + new Vector2<int>(9);

        //[Benchmark]
        public Vector3<int> Generic3() => new Vector3<int>(9) + new Vector3<int>(9);

        //[Benchmark]
        public Vector4<uint> Generic4() => new Vector4<uint>(9) - new Vector4<uint>(9);

        //[Benchmark]
        public Vector4<float> GenericFloating4() => new Vector4<float>(7f) / new Vector4<float>(7f);

        [Benchmark]
        public Vector4<bool> Generic() => GenericA == GenericB;

        [Benchmark]
        public Vector4<bool> Core()
        {
            Vector<int> a = System.Numerics.Vector.Equals(CoreA, CoreB);

            return new Vector4<bool>(
                !a[0].Equals(default),
                !a[1].Equals(default),
                !a[2].Equals(default),
                !a[3].Equals(default));
        }
    }
}
