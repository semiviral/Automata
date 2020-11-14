using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.OpenGL.Memory;
using Xunit;

namespace Automata.Engine.Tests
{
    public class NativeMemoryPoolTests : IDisposable
    {
        private readonly IntPtr _Pointer;
        private readonly NativeMemoryPool _NativeMemoryPool;

        public unsafe NativeMemoryPoolTests()
        {
            _Pointer = Marshal.AllocHGlobal(2048);
            _NativeMemoryPool = new NativeMemoryPool((byte*)_Pointer, 2048);
        }

        [Fact]
        public void VerifyNativeMemoryPoolCreated()
        {
            Debug.Assert(_NativeMemoryPool.RentedBlocks is 0, "No blocks should be rented at this point.");
            Debug.Assert(_NativeMemoryPool.Size is 2048, "Size should match default value.");
        }

        [Fact]
        public void TestNativePoolMemoryRenting()
        {
            Debug.Assert(_NativeMemoryPool.RentedBlocks is 0, "No blocks should be rented at this point.");
            IMemoryOwner<int> memoryOwner = _NativeMemoryPool.Rent<int>(8u);
            Memory<int> memory = memoryOwner.Memory;

            Debug.Assert(_NativeMemoryPool.RentedBlocks is 1, "One block should be rented at this point.");
            Debug.Assert(memory.IsEmpty is false);
            Debug.Assert(memory.Length is 8);

            memoryOwner.Dispose();
            Debug.Assert(_NativeMemoryPool.RentedBlocks is 0);
        }

        void IDisposable.Dispose()
        {
            Marshal.FreeHGlobal(_Pointer);
            GC.SuppressFinalize(this);
        }
    }
}
