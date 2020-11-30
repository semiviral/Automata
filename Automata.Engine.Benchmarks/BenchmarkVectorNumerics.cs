using System.Numerics;
using Automata.Engine.Numerics;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class BenchmarkVectorNumerics
    {
        private Vector4<double> _GenericA;
        private Vector4<double> _GenericB;
        private Vector<double> _CoreA;
        private Vector<double> _CoreB;

        [GlobalSetup]
        public void Setup()
        {
            _GenericA = Vector4<double>.One;
            _GenericB = new Vector4<double>(4);

            _CoreA = Vector<double>.One;
            _CoreB = new Vector<double>(4);
        }

        [Benchmark]
        public Vector4<double> Generic() => _GenericA / _GenericB;

        [Benchmark]
        public Vector<double> Intrinsic() => _CoreA / _CoreB;
    }
}
