using System.Numerics;
using Automata.Engine.Numerics;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class BenchmarkVectorNumerics
    {
        private Vector4<float> _GenericA;
        private Vector4<float> _GenericB;
        private Vector<double> _CoreA;
        private Vector<double> _CoreB;

        [GlobalSetup]
        public void Setup()
        {
            _GenericA = Vector4<float>.One;
            _GenericB = new Vector4<float>(4);

            _CoreA = Vector<double>.One;
            _CoreB = new Vector<double>(4);
        }

        [Benchmark]
        public Vector4<float> Generic() => _GenericA * _GenericB;

        //[Benchmark]
        public Vector4<float> Intrinsic() => _GenericA / _GenericB;
    }
}
