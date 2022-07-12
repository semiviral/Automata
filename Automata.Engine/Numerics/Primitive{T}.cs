// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace Automata.Engine.Numerics
{
    public readonly ref partial struct Primitive<T> where T : unmanaged
    {
        private readonly T _Value;

        public T Value => _Value;

        public Primitive(T value) => _Value = value;

        public static T operator /(Primitive<T> a, Primitive<T> b) => Divide(a, b);


        #region Conversions

        public static implicit operator Primitive<T>(T value) => new Primitive<T>(value);
        public static implicit operator T(Primitive<T> primitive) => primitive._Value;

        #endregion
    }
}
