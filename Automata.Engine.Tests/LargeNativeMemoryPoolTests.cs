using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Automata.Engine.Memory;
using Xunit;

namespace Automata.Engine.Tests
{
    public class LargeNativeMemoryPoolTests : IDisposable
    {
        private const nuint _POOL_SIZE = 3_000_000_000u;

        private readonly IntPtr _Pointer;
        private readonly NativeMemoryPool _NativeMemoryPool;

        public unsafe LargeNativeMemoryPoolTests()
        {
            _Pointer = Marshal.AllocHGlobal((IntPtr)(ulong)_POOL_SIZE);
            _NativeMemoryPool = new NativeMemoryPool((byte*)_Pointer, _POOL_SIZE);
        }

        [Fact]
        public void VerifyNativeMemoryPoolCreated()
        {
            Debug.Assert(_NativeMemoryPool.RentedBlocks is 0, "No blocks should be rented at this point.");
            Debug.Assert(_NativeMemoryPool.Size is _POOL_SIZE, "Size should match default value.");
        }

        [Fact]
        public void TestNativeMemoryPoolSingleRenting() => RentMemoryAndTest<int>(8);

        [Fact]
        public void TestNativeMemoryPoolMultiSingleRenting()
        {
            RentMemoryAndTest<int>(8);
            RentMemoryAndTest<uint>(8);
            RentMemoryAndTest<long>(8);
            RentMemoryAndTest<ulong>(8);
        }

        private void RentMemoryAndTest<T>(int length) where T : unmanaged
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
