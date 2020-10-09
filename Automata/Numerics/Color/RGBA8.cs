#region

using System.Runtime.InteropServices;

#endregion

namespace Automata.Numerics.Color
{
    /// <summary>
    ///     A color with each component representing an 8-bit unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RGBA8
    {
        #region Fields / Properties

        public static RGBA8 Red => new RGBA8(255, 0, 0, 255);
        public static RGBA8 Green => new RGBA8(0, 255, 0, 255);
        public static RGBA8 Blue => new RGBA8(0, 0, 255, 255);
        public static RGBA8 Black => new RGBA8(0, 0, 0, 255);
        public static RGBA8 White => new RGBA8(255, 255, 255, 255);
        public static RGBA8 Transparent => new RGBA8(0, 0, 0, 0);

        private readonly byte _R;
        private readonly byte _G;
        private readonly byte _B;
        private readonly byte _A;

        public byte R => _R;
        public byte G => _G;
        public byte B => _B;
        public byte A => _A;

        #endregion

        public RGBA8(byte r, byte g, byte b) => (_R, _G, _B, _A) = (r, g, b, 255);
        public RGBA8(byte r, byte g, byte b, byte a) => (_R, _G, _B, _A) = (r, g, b, a);
    }
}
