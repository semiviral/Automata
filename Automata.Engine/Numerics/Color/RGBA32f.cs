#region

using System;
using System.Numerics;
using System.Runtime.InteropServices;

#endregion


namespace Automata.Engine.Numerics.Color
{
    /// <summary>
    ///     A color with each component representing a 32-bit signed floating point value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct RGBA32f
    {
        public static RGBA32f Red => new RGBA32f(1f, 0f, 0f, 1f);
        public static RGBA32f Green => new RGBA32f(0f, 1f, 0f, 1f);
        public static RGBA32f Blue => new RGBA32f(0f, 0f, 1f, 1f);
        public static RGBA32f Black => new RGBA32f(0f, 0f, 0f, 1f);
        public static RGBA32f White => new RGBA32f(1f, 1f, 1f, 1f);
        public static RGBA32f Transparent => new RGBA32f(0f, 0f, 0f, 0f);

        private readonly Vector4 _RawValue;

        public float R => _RawValue.X;
        public float G => _RawValue.Y;
        public float B => _RawValue.Z;
        public float A => _RawValue.W;

        private RGBA32f(Vector4 rawValue) => _RawValue = rawValue;

        public RGBA32f(float r, float g, float b) : this(new Vector4(r, g, b, 1f)) { }
        public RGBA32f(float r, float g, float b, float a) : this(new Vector4(r, g, b, a)) { }

        public void CopyTo(float[] array) => CopyTo(array, 0);

        public void CopyTo(float[] array, int index)
        {
            if (array == null)
            {
                throw new NullReferenceException($"Argument '{nameof(array)}' cannot be null.");
            }
            else if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Argument not within bounds of given array.");
            }
            else if ((array.Length - index) < 4)
            {
                throw new ArgumentException("Array with given start index not large enough to copy to.");
            }

            _RawValue.CopyTo(array, index);
        }

        public static implicit operator Vector4(RGBA32f color) => color._RawValue;
        public static explicit operator RGBA32f(Vector4 a) => new RGBA32f(a);
    }
}
