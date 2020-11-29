namespace Automata.Engine.Numerics
{
    public readonly struct Vector4<T> where T : unmanaged
    {
        public static Vector4<T> Zero=> new Vector4<T>(default);
        public static Vector4<T> One => new Vector4<T>(Primitive<T>.One);
        public static Vector4<T> UnitX => new Vector4<T>(Primitive<T>.One, default, default, default);
        public static Vector4<T> UnitY => new Vector4<T>(default, Primitive<T>.One, default, default);
        public static Vector4<T> UnitZ => new Vector4<T>(default, default, Primitive<T>.One, default);
        public static Vector4<T> UnitW => new Vector4<T>(default, default, default, Primitive<T>.One);

        private readonly T _X;
        private readonly T _Y;
        private readonly T _Z;
        private readonly T _W;

        public T X => _X;
        public T Y => _Y;
        public T Z => _Z;
        public T W => _W;

        public Vector4(T xyzw)
        {
            _X = xyzw;
            _Y = xyzw;
            _Z = xyzw;
            _W = xyzw;
        }

        public Vector4(T x, T y, T z, T w)
        {
            _X = x;
            _Y = y;
            _Z = z;
            _W = w;
        }


        #region Operators

        public static Vector4<T> operator *(Vector4<T> a, T b) => a * new Vector4<T>(b);
        public static Vector4<T> operator *(T a, Vector4<T> b) => new Vector4<T>(a) * b;
        public static Vector4<T> operator *(Vector4<T> a, Vector4<T> b) => Vector.MultiplyInternal(a, b);

        #endregion
    }
}
