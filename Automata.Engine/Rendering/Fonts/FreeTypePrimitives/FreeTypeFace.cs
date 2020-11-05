using System.Runtime.InteropServices;
using FreeTypeLong = System.IntPtr;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct GenericContainer
    {
        public readonly FreeTypeLong Data;
        public readonly FreeTypeLong Finalizer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FreeTypeFace
    {
        public readonly int FacesCount;
        public readonly int FaceIndex;

        public readonly FaceFlags FaceFlags;
        public readonly StyleFlags StyleFlags;

        public readonly int GlyphCount;

        public readonly FreeTypeLong FamilyName;
        public readonly FreeTypeLong StyleName;

        public readonly int FixedSizesCount;
        public readonly FreeTypeLong AvailableSizes;

        public readonly int CharmapCount;
        public readonly FreeTypeLong Charmaps;

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

        public readonly FreeTypeLong Glyph;
        public readonly FreeTypeLong Size;
        public readonly FreeTypeLong Charmap;

        private readonly FreeTypeLong Driver;
        private readonly FreeTypeLong Memory;
        private readonly FreeTypeLong Stream;

        private readonly FreeTypeLong SizesList;
        private readonly GenericContainer AutoHint;
        private readonly FreeTypeLong Extensions;

        private readonly FreeTypeLong @internal;
    }
}
