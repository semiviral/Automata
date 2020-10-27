using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Extensions
{
    public static class StructExtensions
    {
        public static unsafe T1 SetValue<T1, T2>(this T1 a, int index, T2 newValue)
            where T1 : unmanaged
            where T2 : unmanaged
        {
            Span<T2> val = MemoryMarshal.Cast<T1, T2>(stackalloc T1[]
            {
                a
            });

            val[index] = newValue;

            return MemoryMarshal.Cast<T2, T1>(val)[0];
        }

        public static T2 GetValue<T1, T2>(this T1 a, int index)
            where T1 : unmanaged
            where T2 : unmanaged
        {
            return MemoryMarshal.Cast<T1, T2>(stackalloc T1[]
            {
                a
            })[index];
        }
    }
}
