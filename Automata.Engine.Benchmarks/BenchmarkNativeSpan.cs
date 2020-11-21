using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Automata.Engine.Memory;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    public class BenchmarkNativeSpan
    {
        private uint[] _Array = null!;

        [GlobalSetup]
        public void Setup() => _Array = Enumerable.Repeat(1u, 1000).ToArray();

        // [Benchmark]
        public Span<uint> SpanCreate() => new Span<uint>(_Array);

        // [Benchmark]
        public NativeSpan<uint> NativeCreate() => new NativeSpan<uint>(_Array);

        //[Benchmark]
        public NativeSpan<uint> NativeCopyTo()
        {
            NativeSpan<uint> span1 = _Array;
            NativeSpan<uint> span2 = _Array;
            span1.CopyTo(span2);
            return span2;
        }

        //[Benchmark]
        public Span<uint> SpanCopyTo()
        {
            Span<uint> span1 = _Array;
            Span<uint> span2 = _Array;
            span1.CopyTo(span2);
            return span2;
        }

        [Benchmark, SkipLocalsInit]
        public NativeSpan<uint> NativeClear()
        {
            NativeSpan<uint> span = _Array;
            span.Clear();
            return span;
        }

        [Benchmark, SkipLocalsInit]
        public Span<uint> SpanClear()
        {
            Span<uint> span = _Array;
            span.Clear();
            return span;
        }
    }
}
