using System.Runtime.Intrinsics.X86;

namespace Automata.Engine.Numerics
{
    public readonly struct Vector3<T> where T : unmanaged
    {
        public static Vector3<T> One => new Vector3<T>(Primitive<T>.One);
        public static Vector3<T> UnitX => new Vector3<T>(Primitive<T>.One, default, default);
        public static Vector3<T> UnitY => new Vector3<T>(default, Primitive<T>.One, default);
        public static Vector3<T> UnitZ => new Vector3<T>(default, default, Primitive<T>.One);

        private readonly T _X;
        private readonly T _Y;
        private readonly T _Z;

        public T X => _X;
        public T Y => _Y;
        public T Z => _Z;

        public Vector3(T xyz)
        {
            _X = xyz;
            _Y = xyz;
            _Z = xyz;
        }

        public Vector3(T x, T y, T z)
        {
            _X = x;
            _Y = y;
            _Z = z;
        }


        #region Operators

        public static Vector3<T> operator *(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(int)) && Sse41.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Multiply(a.AsVector128<T, float>(), b.AsVector128<T, float>()).AsVector3<float, T>();
            }
            else
            {
                Vector.ThrowNotSupportedGenericType();
                return default;
            }
        }

        #endregion
    }
}
