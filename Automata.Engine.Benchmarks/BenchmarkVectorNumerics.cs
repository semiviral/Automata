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
        private Vector4<int> _GenericA;
        private Vector4<int> _GenericB;
        private Vector<double> _CoreA;
        private Vector<double> _CoreB;

        [GlobalSetup]
        public void Setup()
        {
            _GenericA = Vector4<int>.One;
            _GenericB = new Vector4<int>(4);

            _CoreA = Vector<double>.One;
            _CoreB = new Vector<double>(4);
        }

        [Benchmark]
        public Vector4<int> Generic() => _GenericA / _GenericB;

    }
}
