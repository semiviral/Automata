using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Automata.Engine.Extensions
{
    public static class UnmanagedExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetComponent<T, TComponent>(this T a, int index, TComponent newValue)
            where T : unmanaged
            where TComponent : unmanaged
        {
            ref TComponent component = ref Unsafe.Add(ref Unsafe.As<T, TComponent>(ref a), index);
            Unsafe.WriteUnaligned(ref Unsafe.As<TComponent, byte>(ref component), newValue);
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithComponent<T, TComponent>(this T a, int index)
            where T : unmanaged
            where TComponent : unmanaged
        {
            T result = new T();

            ref TComponent aComponent = ref a.GetComponent<T, TComponent>(index);
            ref TComponent resultComponent = ref result.GetComponent<T, TComponent>(index);

            Unsafe.WriteUnaligned(ref Unsafe.As<TComponent, byte>(ref resultComponent), aComponent);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReplaceComponent<T, TComponent>(this T a, int index, TComponent value)
            where T : unmanaged
            where TComponent : unmanaged

        {
            T result = a;

            Unsafe.WriteUnaligned(ref Unsafe.As<TComponent, byte>(ref a.GetComponent<T, TComponent>(index)), value);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref TComponent GetComponent<T, TComponent>(ref this T a, int index) where T : unmanaged where TComponent : unmanaged =>
            ref Unsafe.Add(ref Unsafe.As<T, TComponent>(ref a), index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<TComponent> Unroll<T, TComponent>(this T a)
            where T : unmanaged
            where TComponent : unmanaged =>
            MemoryMarshal.CreateSpan(ref Unsafe.As<T, TComponent>(ref a), sizeof(T) / sizeof(TComponent));
    }
}
