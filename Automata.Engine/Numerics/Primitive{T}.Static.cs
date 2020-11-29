using System;

namespace Automata.Engine.Numerics
{
    public readonly partial struct Primitive<T> where T : unmanaged
    {
        public static readonly T One;

        static Primitive()
        {
            ThrowIfUnsupportedType();

            if (typeof(T) == typeof(int))
            {
                One = (T)(object)1;
            }
        }

        public static void ThrowIfUnsupportedType()
        {
            if ((typeof(T) != typeof(int)) && (typeof(T) != typeof(float)))
            {
                throw new NotSupportedException("Primitive only supports primitives for generic parameter.");
            }
        }
    }
}
