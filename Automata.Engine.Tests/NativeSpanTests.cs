using System.Diagnostics;
using Automata.Engine.Memory;
using Xunit;

namespace Automata.Engine.Tests
{
    public class NativeSpanTests
    {
        [Fact]
        public unsafe void NativeSpanCreationFromNothing()
        {
            NativeSpan<uint> nativeSpan = new NativeSpan<uint>((uint*)0, 2u);

            Debug.Assert(nativeSpan.Length is 2u);
        }

        public unsafe void NativeSpanCreationFromArray()
        {
            NativeSpan<uint> nativeSpan = new NativeSpan<uint>()
        }
    }
}
