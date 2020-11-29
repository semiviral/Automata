using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

// ReSharper disable ConvertToAutoProperty

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vector2<T> where T : unmanaged
    {
        public static Vector2<T> One => new Vector2<T>(Primitive<T>.One);
        public static Vector2<T> UnitX => new Vector2<T>(Primitive<T>.One, default);
        public static Vector2<T> UnitY => new Vector2<T>(default, Primitive<T>.One);

        private readonly T _X;
        private readonly T _Y;

        public T X => _X;
        public T Y => _Y;

        public Vector2(T xy)
        {
            _X = xy;
            _Y = xy;
        }

        public Vector2(T x, T y)
        {
            _X = x;
            _Y = y;
        }


        #region Operators

        public static Vector2<T> operator *(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(int)) && Sse41.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return (a.AsIntrinsic() * b.AsIntrinsic()).AsGeneric<T>();
            }
            else
            {
                // todo throw
                return default;
            }
        }

        #endregion
    }
}
