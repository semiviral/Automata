using System;
using System.Diagnostics;
using System.Linq;
using Automata.Engine.Memory;
using Xunit;

namespace Automata.Engine.Tests
{
    public class NativeSpanTests
    {
        [Fact]
        public unsafe void Constructor_FromPointer()
        {
            uint* ptr = stackalloc[]
            {
                1u,
                2u
            };

            NativeSpan<uint> native_span = new NativeSpan<uint>(ptr, 2u);

            Debug.Assert(native_span.Length is 2u);
            Debug.Assert(native_span[0u] is 1u);
            Debug.Assert(native_span[1u] is 2u);
        }

        [Fact]
        public void Constructor_FromArray()
        {
            uint[] array =
            {
                0u,
                1u
            };

            NativeSpan<uint> native_span = array;

            Debug.Assert(native_span.Length is 2u);
            Debug.Assert(native_span[0u] is 0u);
            Debug.Assert(native_span[1u] is 1u);
        }

        [Fact]
        public void Constructor_FromSpan()
        {
            uint[] array =
            {
                0u,
                1u
            };

            Span<uint> temp = new Span<uint>(array);
            NativeSpan<uint> native_span = new NativeSpan<uint>(temp);

            Debug.Assert(native_span.Length is 2u);
            Debug.Assert(native_span[0u] is 0u);
            Debug.Assert(native_span[1u] is 1u);
        }

        [Fact]
        public void Clear()
        {
            uint[] array = Enumerable.Repeat(2u, 17).ToArray();

            NativeSpan<uint> span = array;
            span.Clear();

            foreach (uint @uint in span)
            {
                Debug.Assert(@uint is 0u);
            }
        }

        [Fact]
        public void ToArray()
        {
            uint[] array =
            {
                1u,
                2u
            };

            NativeSpan<uint> span1 = new NativeSpan<uint>(array);
            uint[] to_array = span1.ToArray();

            Debug.Assert(to_array.Length == array.Length);
            Debug.Assert(to_array[0] is 1u);
            Debug.Assert(to_array[1] is 2u);
        }

        [Fact]
        public void CopyTo()
        {
            uint[] array1 =
            {
                0u,
                0u
            };

            uint[] array2 =
            {
                1u,
                2u
            };

            NativeSpan<uint> native_span1 = new NativeSpan<uint>(array1);
            NativeSpan<uint> native_span2 = new NativeSpan<uint>(array2);

            native_span2.CopyTo(native_span1);

            Debug.Assert(native_span2.Length is 2u);
            Debug.Assert(native_span2[0u] is 1u);
            Debug.Assert(native_span2[1u] is 2u);
        }
    }
}
