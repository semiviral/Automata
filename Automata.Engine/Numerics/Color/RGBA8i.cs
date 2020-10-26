#region

using System.Runtime.InteropServices;

#endregion


namespace Automata.Engine.Numerics.Color
{
    /// <summary>
    ///     A color with each component representing an 8-bit unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RGBA8i
    {
        #region Fields / Properties

        public static RGBA8i Red => new RGBA8i(255, 0, 0, 255);
        public static RGBA8i Green => new RGBA8i(0, 255, 0, 255);
        public static RGBA8i Blue => new RGBA8i(0, 0, 255, 255);
        public static RGBA8i Black => new RGBA8i(0, 0, 0, 255);
        public static RGBA8i White => new RGBA8i(255, 255, 255, 255);
        public static RGBA8i Transparent => new RGBA8i(0, 0, 0, 0);

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public byte A { get; }

        #endregion


        public RGBA8i(byte r, byte g, byte b) => (R, G, B, A) = (r, g, b, 255);
        public RGBA8i(byte r, byte g, byte b, byte a) => (R, G, B, A) = (r, g, b, a);
    }
}
