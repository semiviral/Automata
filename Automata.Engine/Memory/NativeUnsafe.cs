using System.Runtime.CompilerServices;

namespace Automata.Engine.Memory
{
    public static unsafe class NativeUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Add<T>(T* pointer, nuint length) where T : unmanaged => pointer + (length * (nuint)sizeof(T));
    }
}
