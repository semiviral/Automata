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
        internal readonly long LinearHorizonAdvance;
        internal readonly long LinearVerticalAdvance;
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

        internal readonly long LSBDelta;
        internal readonly long RSBDelta;

        internal readonly IntPtr Other;

        private readonly IntPtr @internal;
    }
}
