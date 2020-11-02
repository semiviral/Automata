using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeType
{[StructLayout(LayoutKind.Sequential)]
    internal readonly struct FreeTypeBitmap
    {
        internal readonly int Rows;
        internal readonly int Width;
        internal readonly int Pitch;
        internal readonly IntPtr Buffer;
        internal readonly short GrayCount;
        internal readonly PixelMode PixelMode;
        internal readonly byte PaletteMode;
        internal readonly IntPtr Palette;
    }
}
