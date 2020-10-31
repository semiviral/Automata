using System.Runtime.CompilerServices;

namespace Automata.Engine.Extensions
{
    public static class StructExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T SetValue<T, TComponent>(this T a, int index, TComponent newValue)
            where T : unmanaged
            where TComponent : unmanaged
        {
            byte* ptr = (byte*)&a;

            int byteIndex = index * sizeof(TComponent);
            Unsafe.Write(&ptr[byteIndex], Unsafe.Read<TComponent>(&ptr[byteIndex]));

            return Unsafe.Read<T>(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T WithValue<T, TComponent>(this T a, int index)
            where T : unmanaged
            where TComponent : unmanaged

        {
            byte* ptr = (byte*)&a;
            T result = new T();
            byte* resultPtr = (byte*)&result;

            int byteIndex = index * sizeof(TComponent);
            Unsafe.Write(&resultPtr[byteIndex], Unsafe.Read<TComponent>(&ptr[byteIndex]));

            return Unsafe.Read<T>(resultPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe TComponent GetValue<T, TComponent>(this T a, int index)
            where T : unmanaged
            where TComponent : unmanaged =>
            Unsafe.Read<TComponent>(&((byte*)&a)[index * sizeof(TComponent)]);
    }
}
