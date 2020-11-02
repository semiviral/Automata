using System;
using System.Runtime.InteropServices;
using Automata.Engine.Extensions;
using Automata.Engine.Rendering.Fonts.FreeTypePrimitives;

namespace Automata.Engine.Rendering.Fonts
{
    public class Glyph
    {
        private IntPtr _Handle;
        private FreeTypeGlyph _Glyph;

        private readonly FontLibrary _ParentLibrary;
        private readonly FontFace _ParentFace;

        internal IntPtr Handle
        {
            get => _Handle;
            set
            {
                _Handle = value;
                _Glyph = Marshal.PtrToStructure<FreeTypeGlyph>(value);
            }
        }

        internal Glyph(IntPtr handle, FontLibrary parentLibrary, FontFace parentFace)
        {
            _Handle = handle;
            _ParentLibrary = parentLibrary;
            _ParentFace = parentFace;
        }

        public GlyphMetrics Metrics() => new GlyphMetrics(_Glyph.Metrics);
        public GlyphBitmap Bitmap() => new GlyphBitmap(Handle.FieldOffset<FreeTypeGlyph>(nameof(_Glyph.Bitmap)), _Glyph.Bitmap, _ParentLibrary);
        public Glyph Next() => new Glyph(_Glyph.Next, _ParentLibrary, _ParentFace);
    }
}
