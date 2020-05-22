using System.Runtime.InteropServices;

namespace Automata.Numerics.Color
{
    /// <summary>
    ///     A color with each component representing an 8-bit unsigned value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Color8
    {
        #region Fields / Properties

        public static Color8 Red => new Color8(255, 0, 0, 255);
        public static Color8 Green => new Color8(0, 255, 0, 255);
        public static Color8 Blue => new Color8(0, 0, 255, 255);
        public static Color8 Black => new Color8(0, 0, 0, 255);
        public static Color8 White => new Color8(255, 255, 255, 255);
        public static Color8 Transparent => new Color8(0, 0, 0, 0);

        private readonly byte _R;
        private readonly byte _G;
        private readonly byte _B;
        private readonly byte _A;

        public byte R => _R;
        public byte G => _G;
        public byte B => _B;
        public byte A => _A;

        #endregion

        public Color8(byte r, byte g, byte b) => (_R, _G, _B, _A) = (r, g, b, 255);
        public Color8(byte r, byte g, byte b, byte a) => (_R, _G, _B, _A) = (r, g, b, a);
    }
}
