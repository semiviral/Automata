using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FreeTypeGlyph
    {
        internal readonly IntPtr Library;
        internal readonly IntPtr Face;
        internal readonly IntPtr Next;
        internal readonly uint Reserved;
        internal readonly GenericContainer Generic;

        internal readonly FreeTypeGlyphMetrics Metrics;
        internal readonly Fixed266 LinearHorizonAdvance;
        internal readonly Fixed266 LinearVerticalAdvance;
        internal readonly Vector2x266 Advance;

        internal readonly GlyphFormat Format;

        internal readonly FreeTypeBitmap Bitmap;
        internal readonly int BitmapLeft;
        internal readonly int BitmapTop;

        internal readonly FreeTypeOutline Outline;

        internal readonly uint SubGlyphCount;
        internal readonly IntPtr SubGlyphs;

        internal readonly IntPtr ControlData;
        internal readonly long ControlLength;

        internal readonly int LSBDelta;
        internal readonly int RSBDelta;

        internal readonly IntPtr Other;

        private readonly IntPtr @internal;
    }
}
