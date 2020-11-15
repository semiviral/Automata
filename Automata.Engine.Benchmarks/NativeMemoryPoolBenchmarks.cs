using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Automata.Engine.Memory;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    [RPlotExporter]
    public class NativeMemoryPoolBenchmarks
    {
        private IntPtr? _Pointer;
        private NativeMemoryPool? _NativeMemoryPool;

        [GlobalSetup]
        public unsafe void Setup()
        {
            _Pointer = Marshal.AllocHGlobal((IntPtr)3_000_000_000);
            _NativeMemoryPool = new NativeMemoryPool((byte*)_Pointer, 3_000_000_000);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            if (_Pointer.HasValue) Marshal.FreeHGlobal(_Pointer.Value);
        }

        [Benchmark]
        public IMemoryOwner<int> Rent1KBNoClear()
        {
            IMemoryOwner<int> memoryOwner = _NativeMemoryPool!.Rent<int>(1_000);
            memoryOwner.Dispose();
            return memoryOwner;
        }

        [Benchmark]
        public IMemoryOwner<int> Rent1KBClear()
        {
            IMemoryOwner<int> memoryOwner = _NativeMemoryPool!.Rent<int>(1_000, true);
            memoryOwner.Dispose();
            return memoryOwner;
        }

        [Benchmark]
        public IMemoryOwner<int> Rent1MBNoClear()
        {
            IMemoryOwner<int> memoryOwner = _NativeMemoryPool!.Rent<int>(1_000_000);
            memoryOwner.Dispose();
            return memoryOwner;
        }

        [Benchmark]
        public IMemoryOwner<int> Rent1MBClear()
        {
            IMemoryOwner<int> memoryOwner = _NativeMemoryPool!.Rent<int>(1_000_000, true);
            memoryOwner.Dispose();
            return memoryOwner;
        }
    }
}
