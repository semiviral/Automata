using System;
using System.Runtime.InteropServices;

using FreeTypeLong = System.IntPtr;


namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct GenericContainer
    {
        public readonly IntPtr Data;
        public readonly IntPtr Finalizer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FreeTypeFace
    {
        public readonly int FacesCount;
        public readonly int FaceIndex;

        public readonly FaceFlags FaceFlags;
        public readonly StyleFlags StyleFlags;

        public readonly int GlyphCount;

        public readonly IntPtr FamilyName;
        public readonly IntPtr StyleName;

        public readonly int FixedSizesCount;
        public readonly IntPtr AvailableSizes;

        public readonly int CharmapCount;
        public readonly IntPtr Charmaps;

        public readonly GenericContainer Generic;

        public readonly FreeTypeBounds Bounds;

        public readonly ushort UnitsPerEM;
        public readonly short Ascender;
        public readonly short Descender;
        public readonly short Height;

        public readonly short MaxAdvanceWidth;
        public readonly short MaxAdvanceHeight;

        public readonly short UnderlinePosition;
        public readonly short UnderlineThickness;

        public readonly IntPtr Glyph;
        public readonly IntPtr Size;
        public readonly IntPtr Charmap;

        private readonly IntPtr Driver;
        private readonly IntPtr Memory;
        private readonly IntPtr Stream;

        public readonly IntPtr SizesList;

        public readonly GenericContainer AutoHint;
        public readonly IntPtr Extensions;

        private readonly IntPtr @internal;
    }
}
