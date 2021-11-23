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
        public void TestSingleRentLarge() => RentMemoryAndTest<byte>(1_000_000);

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

            int rented_blocks_before = _NativeMemoryPool.RentedBlocks;

            IMemoryOwner<int> memory_owner1 = _NativeMemoryPool.Rent<int>(length, 0u, out _);
            Memory<int> memory1 = memory_owner1.Memory;

            IMemoryOwner<int> memory_owner2 = _NativeMemoryPool.Rent<int>(length, 0u, out _);
            Memory<int> memory2 = memory_owner1.Memory;

            IMemoryOwner<int> memory_owner3 = _NativeMemoryPool.Rent<int>(length, 0u, out _);
            Memory<int> memory3 = memory_owner1.Memory;

            IMemoryOwner<int> memory_owner4 = _NativeMemoryPool.Rent<int>(length, 0u, out _);
            Memory<int> memory4 = memory_owner1.Memory;

            IMemoryOwner<int> memory_owner5 = _NativeMemoryPool.Rent<int>(length, 0u, out _);
            Memory<int> memory5 = memory_owner1.Memory;

            ValidateMemory(memory1, length);
            ValidateMemory(memory2, length);
            ValidateMemory(memory3, length);
            ValidateMemory(memory4, length);
            ValidateMemory(memory5, length);

            Debug.Assert(_NativeMemoryPool.RentedBlocks == (rented_blocks_before + 5), "Native memory pool should have +5 active rents.");

            memory_owner1.Dispose();
            memory_owner2.Dispose();
            memory_owner3.Dispose();
            memory_owner4.Dispose();
            memory_owner5.Dispose();

            Debug.Assert(_NativeMemoryPool.RentedBlocks == rented_blocks_before, $"{_NativeMemoryPool.RentedBlocks} should be equivalent to the starting value.");
        }

        [Fact]
        public void TestRentingAlignment()
        {
            using IMemoryOwner<uint> memory_owner1 = _NativeMemoryPool.Rent<uint>(1, (uint)sizeof(uint), out nuint index1);
            using IMemoryOwner<ushort> memory_owner2 = _NativeMemoryPool.Rent<ushort>(1, 0u, out nuint index2);
            using IMemoryOwner<uint> memory_owner3 = _NativeMemoryPool.Rent<uint>(1, (uint)sizeof(uint), out nuint index3);
            using IMemoryOwner<byte> memory_owner4 = _NativeMemoryPool.Rent<byte>(7, 0u, out nuint index4);
            using IMemoryOwner<uint> memory_owner5 = _NativeMemoryPool.Rent<uint>(1, (uint)sizeof(uint), out nuint index5);

            Debug.Assert(index1 == 0u);
            Debug.Assert(index2 == 4u);
            Debug.Assert(index3 == 8u);
            Debug.Assert(index4 == 12u);
            Debug.Assert(index5 == 20u);
        }

        [Fact]
        public void TestMultiRentAndReturnWithValidation()
        {
            using IMemoryOwner<byte> memory_owner1 = _NativeMemoryPool.Rent<byte>(1000, (nuint)sizeof(double), out _);
            using IMemoryOwner<int> memory_owner2 = _NativeMemoryPool.Rent<int>(1000, (nuint)sizeof(double), out _);
            using IMemoryOwner<ushort> memory_owner3 = _NativeMemoryPool.Rent<ushort>(1000, (nuint)sizeof(double), out _);
            using IMemoryOwner<short> memory_owner40 = _NativeMemoryPool.Rent<short>(1000, (nuint)sizeof(uint), out _);
            _NativeMemoryPool.Rent<short>(1000, (nuint)sizeof(uint), out _).Dispose();
            _NativeMemoryPool.Rent<short>(10001, 0u, out _).Dispose();
            using IMemoryOwner<float> memory_owner5 = _NativeMemoryPool.Rent<float>(1000, (nuint)sizeof(double), out _);
            using IMemoryOwner<uint> memory_owner6 = _NativeMemoryPool.Rent<uint>(1000, (nuint)sizeof(ushort), out _);
            using IMemoryOwner<ulong> memory_owner7 = _NativeMemoryPool.Rent<ulong>(1000, (nuint)sizeof(double), out _);

            _NativeMemoryPool.ValidateBlocks();
        }

        [Fact]
        public void TestRentAndDisposeEmptiesMemory()
        {
            IMemoryOwner<short> dispose_test = _NativeMemoryPool.Rent<short>(10001, 0u, out _);
            Debug.Assert(dispose_test.Memory.Length > 0u);
            dispose_test.Dispose();
            Debug.Assert(dispose_test.Memory.IsEmpty);
        }

        private void RentMemoryAndTest<T>(int length) where T : unmanaged
        {
            int rented_blocks_before = _NativeMemoryPool.RentedBlocks;

            IMemoryOwner<T> memory_owner = _NativeMemoryPool.Rent<T>(length, 0u, out _);
            Memory<T> memory = memory_owner.Memory;

            Debug.Assert(_NativeMemoryPool.RentedBlocks == (rented_blocks_before + 1), "One block should be rented at this point.");
            ValidateMemory(memory, length);

            memory_owner.Dispose();
            Debug.Assert(_NativeMemoryPool.RentedBlocks == rented_blocks_before, $"{_NativeMemoryPool.RentedBlocks} should be equivalent to the starting value.");
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
