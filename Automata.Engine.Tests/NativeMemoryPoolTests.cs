using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Automata.Engine.Memory;
using Xunit;

namespace Automata.Engine.Tests
{
    public class NativeMemoryPoolTests : IDisposable
    {
        // 3GB
        private const nuint _POOL_SIZE = 3u * 1024u * 1024u * 1024u;

        private readonly IntPtr _Pointer;
        private readonly NativeMemoryPool _NativeMemoryPool;

        public unsafe NativeMemoryPoolTests()
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
        public void TestSingleRent() => RentMemoryAndTest<int>(8);

        [Fact]
        public void TestSingleRent1GB() => RentMemoryAndTest<byte>(1024 * 1024 * 1024);

        [Fact]
        public void TestMultiRent()
        {
            RentMemoryAndTest<int>(8);
            RentMemoryAndTest<uint>(8);
            RentMemoryAndTest<long>(8);
            RentMemoryAndTest<ulong>(8);
        }

        [Fact]
        public void TestMultiRentSequential()
        {
            const int length = 1024;

            int rentedBlocksBefore = _NativeMemoryPool.RentedBlocks;

            IMemoryOwner<int> memoryOwner1 = _NativeMemoryPool.Rent<int>(length);
            Memory<int> memory1 = memoryOwner1.Memory;

            IMemoryOwner<int> memoryOwner2 = _NativeMemoryPool.Rent<int>(length);
            Memory<int> memory2 = memoryOwner1.Memory;

            IMemoryOwner<int> memoryOwner3 = _NativeMemoryPool.Rent<int>(length);
            Memory<int> memory3 = memoryOwner1.Memory;

            IMemoryOwner<int> memoryOwner4 = _NativeMemoryPool.Rent<int>(length);
            Memory<int> memory4 = memoryOwner1.Memory;

            IMemoryOwner<int> memoryOwner5 = _NativeMemoryPool.Rent<int>(length);
            Memory<int> memory5 = memoryOwner1.Memory;

            ValidateMemory(memory1, length);
            ValidateMemory(memory2, length);
            ValidateMemory(memory3, length);
            ValidateMemory(memory4, length);
            ValidateMemory(memory5, length);

            Debug.Assert(_NativeMemoryPool.RentedBlocks == (rentedBlocksBefore + 5), "Native memory pool should have +5 active rents.");

            memoryOwner1.Dispose();
            memoryOwner2.Dispose();
            memoryOwner3.Dispose();
            memoryOwner4.Dispose();
            memoryOwner5.Dispose();

            Debug.Assert(_NativeMemoryPool.RentedBlocks == rentedBlocksBefore, $"{_NativeMemoryPool.RentedBlocks} should be equivalent to the starting value.");
        }

        private void RentMemoryAndTest<T>(int length) where T : unmanaged
        {
            int rentedBlocksBefore = _NativeMemoryPool.RentedBlocks;

            IMemoryOwner<T> memoryOwner = _NativeMemoryPool.Rent<T>(length);
            Memory<T> memory = memoryOwner.Memory;

            Debug.Assert(_NativeMemoryPool.RentedBlocks == (rentedBlocksBefore + 1), "One block should be rented at this point.");
            ValidateMemory(memory, length);

            memoryOwner.Dispose();
            Debug.Assert(_NativeMemoryPool.RentedBlocks == rentedBlocksBefore, $"{_NativeMemoryPool.RentedBlocks} should be equivalent to the starting value.");
        }

        private static void ValidateMemory<T>(Memory<T> memory, int length)
        {
            Debug.Assert(memory.IsEmpty is false, "Memory should not be empty.");
            Debug.Assert(memory.Length == (int)length, "Memory should be of correct, expected length.");
        }

        void IDisposable.Dispose()
        {
            Marshal.FreeHGlobal(_Pointer);
            GC.SuppressFinalize(this);
        }
    }
}
