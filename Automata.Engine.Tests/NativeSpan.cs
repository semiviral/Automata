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

            NativeSpan<uint> nativeSpan = new NativeSpan<uint>(ptr, 2u);

            Debug.Assert(nativeSpan.Length is 2u);
            Debug.Assert(nativeSpan[0u] is 1u);
            Debug.Assert(nativeSpan[1u] is 2u);
        }

        [Fact]
        public void Constructor_FromArray()
        {
            uint[] array =
            {
                0u,
                1u
            };

            NativeSpan<uint> nativeSpan = array;

            Debug.Assert(nativeSpan.Length is 2u);
            Debug.Assert(nativeSpan[0u] is 0u);
            Debug.Assert(nativeSpan[1u] is 1u);
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
            NativeSpan<uint> nativeSpan = new NativeSpan<uint>(temp);

            Debug.Assert(nativeSpan.Length is 2u);
            Debug.Assert(nativeSpan[0u] is 0u);
            Debug.Assert(nativeSpan[1u] is 1u);
        }

        [Fact]
        public void Clear()
        {
            uint[] array = Enumerable.Repeat(2u, 17).ToArray();

            NativeSpan<uint> span = array;
            span.Clear();

            foreach (uint _uint in span)
            {
                Debug.Assert(_uint is 0u);
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
            uint[] toArray = span1.ToArray();

            Debug.Assert(toArray.Length == array.Length);
            Debug.Assert(toArray[0] is 1u);
            Debug.Assert(toArray[1] is 2u);
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

            NativeSpan<uint> nativeSpan1 = new NativeSpan<uint>(array1);
            NativeSpan<uint> nativeSpan2 = new NativeSpan<uint>(array2);

            nativeSpan2.CopyTo(nativeSpan1);

            Debug.Assert(nativeSpan2.Length is 2u);
            Debug.Assert(nativeSpan2[0u] is 1u);
            Debug.Assert(nativeSpan2[1u] is 2u);
        }
    }
}
