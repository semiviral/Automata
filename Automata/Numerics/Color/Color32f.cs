#region

using System;
using System.Numerics;

#endregion

namespace Automata.Numerics.Color
{
    /// <summary>
    ///     A color with each component representing a 32-bit signed floating point value.
    /// </summary>
    public readonly partial struct Color32f
    {
        public static Color32f Red => new Color32f(1f, 0f, 0f, 1f);
        public static Color32f Green => new Color32f(0f, 1f, 0f, 1f);
        public static Color32f Blue => new Color32f(0f, 0f, 1f, 1f);
        public static Color32f Black => new Color32f(0f, 0f, 0f, 1f);
        public static Color32f White => new Color32f(1f, 1f, 1f, 1f);
        public static Color32f Transparent => new Color32f(0f, 0f, 0f, 0f);

        private readonly Vector4 _RawValue;

        public float R => _RawValue.X;
        public float G => _RawValue.Y;
        public float B => _RawValue.Z;
        public float A => _RawValue.W;

        private Color32f(Vector4 rawValue) => _RawValue = rawValue;

        public Color32f(float r, float g, float b) : this(new Vector4(r, g, b, 1f)) { }
        public Color32f(float r, float g, float b, float a) : this(new Vector4(r, g, b, a)) { }

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

        public static implicit operator Vector4(Color32f color) => color._RawValue;
        public static explicit operator Color32f(Vector4 a) => new Color32f(a);
    }
}