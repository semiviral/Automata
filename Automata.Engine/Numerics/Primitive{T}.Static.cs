using System;
using System.Runtime.CompilerServices;

// ReSharper disable CognitiveComplexity

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
                Zero = (T)(object)(ulong)0u;
            }
            else if (typeof(T) == typeof(int))
            {
                One = (T)(object)(int)1;
                Zero = (T)(object)(int)0;
            }
            else if (typeof(T) == typeof(uint))
            {
                One = (T)(object)(uint)1u;
                Zero = (T)(object)(uint)0u;
            }
            else if (typeof(T) == typeof(short))
            {
                One = (T)(object)(short)1;
                Zero = (T)(object)(short)0;
            }
            else if (typeof(T) == typeof(ushort))
            {
                One = (T)(object)(ushort)1u;
                Zero = (T)(object)(ushort)0u;
            }
            else if (typeof(T) == typeof(sbyte))
            {
                One = (T)(object)(sbyte)1;
                Zero = (T)(object)(sbyte)0;
            }
            else if (typeof(T) == typeof(byte))
            {
                One = (T)(object)(byte)1u;
                Zero = (T)(object)(byte)0u;
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
            else if (typeof(T) == typeof(decimal))
            {
                One = (T)(object)1m;
                Zero = (T)(object)0m;
            }
            else if (typeof(T) == typeof(bool))
            {
                One = (T)(object)(bool)true;
                Zero = (T)(object)(bool)false;
            }
            else
            {
                ThrowNotSupportedType();
            }
        }

        private static void ThrowNotSupportedType() => throw new NotSupportedException("Primitive only supports primitives for generic parameter.");

        public static bool IsFloatingPoint() =>
            (typeof(T) == typeof(float))
            || (typeof(T) == typeof(double))
            || (typeof(T) == typeof(decimal));

        public static bool IsIntegral() =>
            (typeof(T) == typeof(sbyte))
            || (typeof(T) == typeof(byte))
            || (typeof(T) == typeof(short))
            || (typeof(T) == typeof(ushort))
            || (typeof(T) == typeof(int))
            || (typeof(T) == typeof(uint))
            || (typeof(T) == typeof(long))
            || (typeof(T) == typeof(ulong));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo Convert<TTo>(T from) where TTo : unmanaged
        {
            // sbyte
            if ((typeof(T) == typeof(sbyte)) && (typeof(TTo) == typeof(float)))
            {
                sbyte temp = (sbyte)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(sbyte)) && (typeof(TTo) == typeof(double)))
            {
                sbyte temp = (sbyte)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(sbyte)) && (typeof(TTo) == typeof(decimal)))
            {
                sbyte temp = (sbyte)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // byte
            else if ((typeof(T) == typeof(byte)) && (typeof(TTo) == typeof(float)))
            {
                byte temp = (byte)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(byte)) && (typeof(TTo) == typeof(double)))
            {
                byte temp = (byte)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(byte)) && (typeof(TTo) == typeof(decimal)))
            {
                byte temp = (byte)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // short
            else if ((typeof(T) == typeof(short)) && (typeof(TTo) == typeof(float)))
            {
                short temp = (short)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(short)) && (typeof(TTo) == typeof(double)))
            {
                short temp = (short)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(short)) && (typeof(TTo) == typeof(decimal)))
            {
                short temp = (short)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // ushort
            else if ((typeof(T) == typeof(ushort)) && (typeof(TTo) == typeof(float)))
            {
                ushort temp = (ushort)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(ushort)) && (typeof(TTo) == typeof(double)))
            {
                ushort temp = (ushort)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(ushort)) && (typeof(TTo) == typeof(decimal)))
            {
                ushort temp = (ushort)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // int
            else if ((typeof(T) == typeof(int)) && (typeof(TTo) == typeof(float)))
            {
                int temp = (int)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(int)) && (typeof(TTo) == typeof(double)))
            {
                int temp = (int)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(int)) && (typeof(TTo) == typeof(decimal)))
            {
                int temp = (int)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // uint
            else if ((typeof(T) == typeof(uint)) && (typeof(TTo) == typeof(float)))
            {
                uint temp = (uint)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(uint)) && (typeof(TTo) == typeof(double)))
            {
                uint temp = (uint)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(uint)) && (typeof(TTo) == typeof(decimal)))
            {
                uint temp = (uint)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // long
            else if ((typeof(T) == typeof(long)) && (typeof(TTo) == typeof(float)))
            {
                long temp = (long)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(long)) && (typeof(TTo) == typeof(double)))
            {
                long temp = (long)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(long)) && (typeof(TTo) == typeof(decimal)))
            {
                long temp = (long)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // ulong
            else if ((typeof(T) == typeof(ulong)) && (typeof(TTo) == typeof(float)))
            {
                ulong temp = (ulong)(object)from;
                return (TTo)(object)(float)temp;
            }
            else if ((typeof(T) == typeof(ulong)) && (typeof(TTo) == typeof(double)))
            {
                ulong temp = (ulong)(object)from;
                return (TTo)(object)(double)temp;
            }
            else if ((typeof(T) == typeof(ulong)) && (typeof(TTo) == typeof(decimal)))
            {
                ulong temp = (ulong)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // float

            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(sbyte)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(sbyte)temp;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(byte)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(byte)temp;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(short)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(short)temp;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(ushort)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(ushort)temp;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(int)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(int)temp;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(uint)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(uint)temp;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(long)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(long)temp;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(ulong)))
            {
                float temp = (float)(object)from;
                return (TTo)(object)(ulong)temp;
            }

            // double
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(sbyte)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(sbyte)temp;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(byte)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(byte)temp;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(short)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(short)temp;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(ushort)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(ushort)temp;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(int)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(int)temp;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(uint)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(uint)temp;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(long)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(long)temp;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(ulong)))
            {
                double temp = (double)(object)from;
                return (TTo)(object)(ulong)temp;
            }

            // decimal
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(sbyte)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(sbyte)temp;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(byte)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(byte)temp;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(short)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(short)temp;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(ushort)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(ushort)temp;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(int)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(int)temp;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(uint)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(uint)temp;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(long)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(long)temp;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(ulong)))
            {
                decimal temp = (decimal)(object)from;
                return (TTo)(object)(ulong)temp;
            }

            // default
            else
            {
                return (TTo)(object)from;
            }
        }

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
