using System;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Numerics
{
    public readonly partial struct Primitive<T> where T : unmanaged
    {
        public static readonly T One;
        public static readonly T Zero;

        static Primitive()
        {
            if (typeof(T) == typeof(long))
            {
                One = (T)(object)(long)1;
                Zero = (T)(object)(long)0;
            }
            else if (typeof(T) == typeof(ulong))
            {
                One = (T)(object)(ulong)1u;
                Zero = (T)(object)(long)0u;
            }
            else if (typeof(T) == typeof(int))
            {
                One = (T)(object)1;
                Zero = (T)(object)0;
            }
            else if (typeof(T) == typeof(uint))
            {
                One = (T)(object)1u;
                Zero = (T)(object)0u;
            }
            else if (typeof(T) == typeof(short))
            {
                One = (T)(object)(short)1;
                Zero = (T)(object)(short)0;
            }
            else if (typeof(T) == typeof(ushort))
            {
                One = (T)(object)(ushort)1;
                Zero = (T)(object)(ushort)0;
            }
            else if (typeof(T) == typeof(sbyte))
            {
                One = (T)(object)(sbyte)1;
                Zero = (T)(object)(sbyte)0;
            }
            else if (typeof(T) == typeof(byte))
            {
                One = (T)(object)(byte)1;
                Zero = (T)(object)(byte)0;
            }
            else if (typeof(T) == typeof(float))
            {
                One = (T)(object)1f;
                Zero = (T)(object)0f;
            }
            else if (typeof(T) == typeof(double))
            {
                One = (T)(object)1d;
                Zero = (T)(object)0d;
            }
            else if (typeof(T) == typeof(bool))
            {
                One = (T)(object)true;
                Zero = (T)(object)false;
            }
            else
            {
                ThrowNotSupportedType();
            }
        }

        private static void ThrowNotSupportedType() => throw new NotSupportedException("Primitive only supports primitives for generic parameter.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Divide(T a, T b)
        {
            if (typeof(T) == typeof(sbyte))
            {
                return (T)(object)((sbyte)(object)a / (sbyte)(object)b);
            }
            else if (typeof(T) == typeof(byte))
            {
                return (T)(object)((byte)(object)a / (byte)(object)b);
            }
            else if (typeof(T) == typeof(short))
            {
                return (T)(object)((short)(object)a / (short)(object)b);
            }
            else if (typeof(T) == typeof(ushort))
            {
                return (T)(object)((ushort)(object)a / (ushort)(object)b);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)((int)(object)a / (int)(object)b);
            }
            else if (typeof(T) == typeof(uint))
            {
                return (T)(object)((uint)(object)a / (uint)(object)b);
            }
            else if (typeof(T) == typeof(long))
            {
                return (T)(object)((long)(object)a / (long)(object)b);
            }
            else if (typeof(T) == typeof(ulong))
            {
                return (T)(object)((ulong)(object)a / (ulong)(object)b);
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)((float)(object)a / (float)(object)b);
            }
            else if (typeof(T) == typeof(double))
            {
                return (T)(object)((double)(object)a / (double)(object)b);
            }
            else if (typeof(T) == typeof(decimal))
            {
                return (T)(object)((decimal)(object)a / (decimal)(object)b);
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }
    }
}
