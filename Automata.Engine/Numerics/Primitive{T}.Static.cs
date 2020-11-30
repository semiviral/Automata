using System;

namespace Automata.Engine.Numerics
{
    public readonly partial struct Primitive<T> where T : unmanaged
    {
        public static readonly T One;

        static Primitive()
        {
            if (typeof(T) == typeof(long))
            {
                One = (T)(object)(long)1;
            }
            else if (typeof(T) == typeof(ulong))
            {
                One = (T)(object)(ulong)1u;
            }
            else if (typeof(T) == typeof(int))
            {
                One = (T)(object)1;
            }
            else if (typeof(T) == typeof(uint))
            {
                One = (T)(object)1u;
            }
            else if (typeof(T) == typeof(short))
            {
                One = (T)(object)(short)1;
            }
            else if (typeof(T) == typeof(ushort))
            {
                One = (T)(object)(ushort)1;
            }
            else if (typeof(T) == typeof(sbyte))
            {
                One = (T)(object)(sbyte)1;
            }
            else if (typeof(T) == typeof(byte))
            {
                One = (T)(object)(byte)1;
            }
            else if (typeof(T) == typeof(float))
            {
                One = (T)(object)1f;
            }
            else if (typeof(T) == typeof(double))
            {
                One = (T)(object)1d;
            }
            else
            {
                ThrowNotSupportedType();
            }
        }

        private static void ThrowNotSupportedType() => throw new NotSupportedException("Primitive only supports primitives for generic parameter.");
    }
}
