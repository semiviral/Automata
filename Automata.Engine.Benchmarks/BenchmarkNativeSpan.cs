using System;
using System.Linq;
using Automata.Engine.Memory;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    public class BenchmarkNativeSpan
    {
        private uint[] _Array1 = null!;
        private uint[] _Array2 = null!;

        [GlobalSetup]
        public void Setup()
        {
            _Array1 = Enumerable.Repeat(1u, 1000).ToArray();
            _Array2 = Enumerable.Repeat(2u, 1000).ToArray();
        }

        [Benchmark]
        public NativeSpan<uint> NativeCopyTo()
        {
            NativeSpan<uint> span1 = _Array1;
            NativeSpan<uint> span2 = _Array1;
            span1.CopyTo(span2);
            return span2;
        }

        [Benchmark]
        public Span<uint> SpanCopyTo()
        {
            Span<uint> span1 = _Array1;
            Span<uint> span2 = _Array1;
            span1.CopyTo(span2);
            return span2;
        }
    }
}
