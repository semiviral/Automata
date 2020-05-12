#region

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

namespace Automata.Numerics
{
    public class VectorConstants
    {
        public const int INTEGER_BOOLEAN_TRUE_VALUE = -1;
        public const int INTEGER_BOOLEAN_FALSE_VALUE = 0;

        public static Vector128<int> EqualsImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        public static Vector128<int> BitwiseAndImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.And(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        public static Vector128<int> BitwiseOrImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Or(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        public static Vector128<int> AddImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Add(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        public static Vector128<int> SubtractImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Subtract(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        public static Vector128<int> MultiplyImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse41.IsSupported)
            {
                // todo this conversion is slow
                return (Vector128<int>)(Vector3i)Sse41.Multiply(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        public static Vector128<int> GreaterThanImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        public static Vector128<int> LessThanImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }
    }
}
