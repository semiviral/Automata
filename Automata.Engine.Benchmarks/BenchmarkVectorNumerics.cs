using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Automata.Engine.Numerics;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class BenchmarkVectorNumerics
    {
        private Vector4<float> _GenericA;
        private Vector4<float> _GenericB;
        private Vector4 IntrinsicA;
        private Vector4 _IntrinsicB;

        [GlobalSetup]
        public void Setup()
        {
            _GenericA = Vector4<float>.One;
            _GenericB = new Vector4<float>(4);

            IntrinsicA = Vector4.One;
            _IntrinsicB = new Vector4(4);
        }

        [Benchmark]
        public Vector4<float> Generic() => _GenericA * _GenericB;

        [Benchmark]
        public Vector4 Intrinsic() => IntrinsicA * _IntrinsicB;

    }
}
