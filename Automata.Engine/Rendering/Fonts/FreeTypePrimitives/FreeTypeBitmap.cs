using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FreeTypeBitmap
    {
        internal readonly uint Rows;
        internal readonly uint Width;
        internal readonly int Pitch;

        internal readonly IntPtr Buffer;
        internal readonly short GrayCount;

        internal readonly PixelMode PixelMode;

        internal readonly byte PaletteMode;
        internal readonly IntPtr Palette;
    }
}
