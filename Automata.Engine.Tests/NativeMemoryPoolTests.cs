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
        public void TestNativeMemoryPoolSingleRenting() => RentMemoryAndTest<int>(8);

        [Fact]
        public void TestNativeMemoryPoolMultiSingleRenting()
        {
            RentMemoryAndTest<int>(8u);
            RentMemoryAndTest<uint>(8u);
            RentMemoryAndTest<long>(8u);
            RentMemoryAndTest<ulong>(8u);
        }

        private void RentMemoryAndTest<T>(nuint length) where T : unmanaged
        {
            int rentedBlocksBefore = _NativeMemoryPool.RentedBlocks;

            IMemoryOwner<T> memoryOwner = _NativeMemoryPool.Rent<T>(length);
            Memory<T> memory = memoryOwner.Memory;

            Debug.Assert(_NativeMemoryPool.RentedBlocks == (rentedBlocksBefore + 1), "One block should be rented at this point.");
            Debug.Assert(memory.IsEmpty is false, "Memory should not be empty.");
            Debug.Assert(memory.Length == (int)length, "Memory should be of correct, expected length.");

            memoryOwner.Dispose();
            Debug.Assert(_NativeMemoryPool.RentedBlocks == rentedBlocksBefore, $"{_NativeMemoryPool.RentedBlocks} should be equivalent to the starting value.");
        }

        void IDisposable.Dispose()
        {
            Marshal.FreeHGlobal(_Pointer);
            GC.SuppressFinalize(this);
        }
    }
}
