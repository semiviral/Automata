using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Benchmarking
{
    internal static class BenchmarkStructExtensionsStatic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe TComponent GetValueRead<T, TComponent>(this T a, int index) where T : unmanaged where TComponent : unmanaged =>
            Unsafe.Read<TComponent>(&((byte*)&a)[index * sizeof(TComponent)]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe TComponent GetValueAs<T, TComponent>(this T a, int index) where T : unmanaged where TComponent : unmanaged =>
            Unsafe.As<byte, TComponent>(ref ((byte*)&a)[index * sizeof(TComponent)]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TComponent GetValueAdd<T, TComponent>(this T a, int index) where T : unmanaged where TComponent : unmanaged =>
            Unsafe.Add(ref Unsafe.As<T, TComponent>(ref a), index);
    }

    [RPlotExporter]
    public class BenchmarkStructExtensions
    {
        private const int _TEST_INT = 0b00000000_11111111_00000000_00000000;

        [Benchmark]
        public bool GetValueRead() => _TEST_INT.GetValueRead<int, byte>(2) == byte.MaxValue;

        [Benchmark]
        public bool GetValueAs() => _TEST_INT.GetValueAs<int, byte>(2) == byte.MaxValue;

        [Benchmark]
        public bool GetValueAdd() => _TEST_INT.GetValueAdd<int, byte>(2) == byte.MaxValue;
    }
}
