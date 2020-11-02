using System;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.Fonts.FreeType;

namespace Automata.Engine.Rendering.Fonts
{
    public class Glyph
    {

        private IntPtr _Handle;
        private FreeTypeGlyph _Glyph;

        private FontLibrary _ParentLibrary;
        private FontFace _ParentFace;

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
        public Glyph Next() => new Glyph(_Glyph.Next, _ParentLibrary, _ParentFace);
    }
}
