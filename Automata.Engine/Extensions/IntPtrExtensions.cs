using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Extensions
{
    public static class IntPtrExtensions
    {
        public static IntPtr FieldOffset<T>(this IntPtr start, string fieldName) where T : unmanaged =>
            new IntPtr(start.ToInt64() + Marshal.OffsetOf<T>(fieldName).ToInt64());

        public static unsafe T* AsPointer<T>(this nuint a) where T : unmanaged => (T*)a;
        public static unsafe T* AsPointer<T>(this nint a) where T : unmanaged => (T*)a;
    }
}
