#region

using System.Runtime.InteropServices;

#endregion


namespace Automata.Engine.Numerics.Color
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

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public byte A { get; }

        #endregion


        public RGBA8(byte r, byte g, byte b) => (R, G, B, A) = (r, g, b, 255);
        public RGBA8(byte r, byte g, byte b, byte a) => (R, G, B, A) = (r, g, b, a);
    }
}
