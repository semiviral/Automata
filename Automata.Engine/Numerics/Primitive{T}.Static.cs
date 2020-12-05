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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFloatingPoint() =>
            (typeof(T) == typeof(float))
            || (typeof(T) == typeof(double))
            || (typeof(T) == typeof(decimal));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public static bool IsSignedIntegral() =>
            (typeof(T) == typeof(sbyte))
            || (typeof(T) == typeof(short))
            || (typeof(T) == typeof(int))
            || (typeof(T) == typeof(long));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnsignedIntegral() =>
            (typeof(T) == typeof(byte))
            || (typeof(T) == typeof(ushort))
            || (typeof(T) == typeof(uint))
            || (typeof(T) == typeof(ulong));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo ConvertSepcialized<TTo>(T from) where TTo : unmanaged => (TTo)(object)(long)(int)(object)from;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe TTo Convert<TTo>(T from) where TTo : unmanaged
        {
            // handle same-type casting
            if (typeof(T) == typeof(TTo))
            {
                return (TTo)(object)from;
            }
            else if (IsIntegral() && Primitive<TTo>.IsIntegral())
            {
                // signed conversions need to do sign carrying operations
                // this is pretty slow, so eventually I'll get around to
                // generic specializing every case.
                if (IsSignedIntegral() && Primitive<TTo>.IsSignedIntegral())
                {
                    // these should be jitted to constants
                    int sign_alignment_shift = (sizeof(TTo) * 8) - 1;
                    nuint sign_extension = (nuint.MaxValue >> (sizeof(T) * 8)) << (sizeof(T) * 8);

                    nuint value = Unsafe.As<T, nuint>(ref from);
                    nuint signByte = (value >> ((sizeof(T) * 8) - 1)) << sign_alignment_shift;
                    value = (value & ~signByte) | signByte;

                    if (value < 0)
                    {
                        value |= sign_extension;
                    }

                    return Unsafe.As<nuint, TTo>(ref value);
                }
                else
                {
                    return Unsafe.Read<TTo>(&from);
                }
            }

            // sbyte
            else if ((typeof(T) == typeof(sbyte)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(sbyte)(object)from;
            }
            else if ((typeof(T) == typeof(sbyte)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(sbyte)(object)from;
            }
            else if ((typeof(T) == typeof(sbyte)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(sbyte)(object)from;
            }

            // byte
            else if ((typeof(T) == typeof(byte)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(byte)(object)from;
            }
            else if ((typeof(T) == typeof(byte)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(byte)(object)from;
            }
            else if ((typeof(T) == typeof(byte)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(byte)(object)from;
            }

            // short
            else if ((typeof(T) == typeof(short)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(short)(object)from;
            }
            else if ((typeof(T) == typeof(short)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(short)(object)from;
            }
            else if ((typeof(T) == typeof(short)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(short)(object)from;
            }

            // ushort
            else if ((typeof(T) == typeof(ushort)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(ushort)(object)from;
            }
            else if ((typeof(T) == typeof(ushort)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(ushort)(object)from;
            }
            else if ((typeof(T) == typeof(ushort)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(ushort)(object)from;
            }

            // int
            else if ((typeof(T) == typeof(int)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(int)(object)from;
            }
            else if ((typeof(T) == typeof(int)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(int)(object)from;
            }
            else if ((typeof(T) == typeof(int)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(int)(object)from;
            }

            // uint
            else if ((typeof(T) == typeof(uint)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(uint)(object)from;
            }
            else if ((typeof(T) == typeof(uint)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(uint)(object)from;
            }
            else if ((typeof(T) == typeof(uint)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(uint)(object)from;
            }

            // long
            else if ((typeof(T) == typeof(long)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(long)(object)from;
            }
            else if ((typeof(T) == typeof(long)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(long)(object)from;
            }
            else if ((typeof(T) == typeof(long)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(long)(object)from;
            }

            // ulong
            else if ((typeof(T) == typeof(ulong)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(ulong)(object)from;
            }
            else if ((typeof(T) == typeof(ulong)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(ulong)(object)from;
            }
            else if ((typeof(T) == typeof(ulong)) && (typeof(TTo) == typeof(decimal)))
            {
                ulong temp = (ulong)(object)from;
                return (TTo)(object)(decimal)temp;
            }

            // float

            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(sbyte)))
            {
                return (TTo)(object)(sbyte)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(byte)))
            {
                return (TTo)(object)(byte)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(short)))
            {
                return (TTo)(object)(short)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(ushort)))
            {
                return (TTo)(object)(ushort)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(int)))
            {
                return (TTo)(object)(int)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(uint)))
            {
                return (TTo)(object)(uint)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(long)))
            {
                return (TTo)(object)(long)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(ulong)))
            {
                return (TTo)(object)(ulong)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(float)(object)from;
            }
            else if ((typeof(T) == typeof(float)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(float)(object)from;
            }

            // double
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(sbyte)))
            {
                return (TTo)(object)(sbyte)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(byte)))
            {
                return (TTo)(object)(byte)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(short)))
            {
                return (TTo)(object)(short)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(ushort)))
            {
                return (TTo)(object)(ushort)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(int)))
            {
                return (TTo)(object)(int)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(uint)))
            {
                return (TTo)(object)(uint)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(long)))
            {
                return (TTo)(object)(long)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(ulong)))
            {
                return (TTo)(object)(ulong)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(double)(object)from;
            }
            else if ((typeof(T) == typeof(double)) && (typeof(TTo) == typeof(decimal)))
            {
                return (TTo)(object)(decimal)(double)(object)from;
            }

            // decimal
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(sbyte)))
            {
                return (TTo)(object)(sbyte)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(byte)))
            {
                return (TTo)(object)(byte)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(short)))
            {
                return (TTo)(object)(short)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(ushort)))
            {
                return (TTo)(object)(ushort)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(int)))
            {
                return (TTo)(object)(int)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(uint)))
            {
                return (TTo)(object)(uint)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(long)))
            {
                return (TTo)(object)(long)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(ulong)))
            {
                return (TTo)(object)(ulong)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(float)))
            {
                return (TTo)(object)(float)(decimal)(object)from;
            }
            else if ((typeof(T) == typeof(decimal)) && (typeof(TTo) == typeof(double)))
            {
                return (TTo)(object)(double)(decimal)(object)from;
            }

            // unsupported conversion
            else
            {
                ThrowNotSupportedType();
                return default;
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
