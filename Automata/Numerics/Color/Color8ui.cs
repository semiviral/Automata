#region

using System.Runtime.InteropServices;

#endregion

namespace Automata.Numerics.Color
{
    /// <summary>
    ///     A color with each component representing an 8-bit unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Color8ui
    {
        #region Fields / Properties

        public static Color8ui Red => new Color8ui(255, 0, 0, 255);
        public static Color8ui Green => new Color8ui(0, 255, 0, 255);
        public static Color8ui Blue => new Color8ui(0, 0, 255, 255);
        public static Color8ui Black => new Color8ui(0, 0, 0, 255);
        public static Color8ui White => new Color8ui(255, 255, 255, 255);
        public static Color8ui Transparent => new Color8ui(0, 0, 0, 0);

        private readonly byte _R;
        private readonly byte _G;
        private readonly byte _B;
        private readonly byte _A;

        public byte R => _R;
        public byte G => _G;
        public byte B => _B;
        public byte A => _A;

        #endregion

        public Color8ui(byte r, byte g, byte b) => (_R, _G, _B, _A) = (r, g, b, 255);
        public Color8ui(byte r, byte g, byte b, byte a) => (_R, _G, _B, _A) = (r, g, b, a);
    }
}
