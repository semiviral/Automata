using System;
using System.Runtime.InteropServices;

using FreeTypeLong = System.IntPtr;
using FreeTypeULong = System.UIntPtr;

namespace Automata.Engine.Rendering.Fonts
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GenericContainer
    {
        public IntPtr Data;
        public IntPtr Finalizer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FreeTypeFace
    {
        public FreeTypeLong FacesCount;
        public FreeTypeLong FaceIndex;

        public FreeTypeLong FaceFlags;
        public FreeTypeLong StyleFlags;

        public FreeTypeLong GlyphCount;

        public IntPtr FamilyName;
        public IntPtr StyleName;

        public int FixedSizesCount;
        public IntPtr AvailableSizes;

        public int CharmapCount;
        public IntPtr Charmaps;

        public FreeTypeBounds Bounds;

        public ushort UnitsPerEM;
        public short Ascender;
        public short Descender;
        public short Height;

        public short MaxAdvanceWidth;
        public short MaxAdvanceHeight;

        public short UnderlinePosition;
        public short UnderlineThickness;

        public IntPtr Glyph;
        public IntPtr Size;
        public IntPtr Charmap;

        private IntPtr Driver;
        private IntPtr Memory;
        private IntPtr Stream;

        public IntPtr SizesList;
        public GenericContainer AutoHint;
        public IntPtr Extensions;

        private IntPtr @internal;
    }
}
