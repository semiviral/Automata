#region

using System;
using System.Numerics;

#endregion

namespace Automata.Rendering
{
    public readonly struct Color64
    {
        public static Color64 Red => new Color64(1f, 0f, 0f, 1f);
        public static Color64 Green => new Color64(0f, 1f, 0f, 1f);
        public static Color64 Blue => new Color64(0f, 0f, 1f, 1f);
        public static Color64 Black => new Color64(0f, 0f, 0f, 1f);
        public static Color64 White => new Color64(1f, 1f, 1f, 1f);
        public static Color64 Transparent => new Color64(0f, 0f, 0f, 0f);

        private Vector4 RawValue { get; }

        public float R => RawValue.X;
        public float G => RawValue.Y;
        public float B => RawValue.Z;
        public float A => RawValue.W;

        public Color64(Vector4 rawValue) => RawValue = rawValue;
        public Color64(float r, float g, float b, float a) : this(Vector4.Clamp(new Vector4(r, g, b, a), Vector4.Zero, Vector4.One)) { }

        public void CopyTo(float[] array) => CopyTo(array, 0);

        public void CopyTo(float[] array, int index)
        {
            if (array == null)
            {
                throw new NullReferenceException($"Argument '{nameof(array)}' cannot be null.");
            }

            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Argument not within bounds of given array.");
            }

            if ((array.Length - index) < 4)
            {
                throw new ArgumentException("Array with given start index not large enough to copy to.");
            }

            RawValue.CopyTo(array, index);
        }

        public static implicit operator Vector4(Color64 color) => color.RawValue;
    }
}
