using System;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.Fonts.FreeType;

namespace Automata.Engine.Rendering.Fonts
{
    public class GlyphMetrics
    {
        private IntPtr _Handle;
        private FreeTypeGlyphMetrics _GlyphMetrics;

        public IntPtr Handle
        {
            get => _Handle;
            set
            {
                _Handle = value;
                _GlyphMetrics = Marshal.PtrToStructure<FreeTypeGlyphMetrics>(value);
            }
        }

        internal GlyphMetrics(IntPtr handle) => Handle = handle;
        internal GlyphMetrics(FreeTypeGlyphMetrics glyphMetrics) => _GlyphMetrics = glyphMetrics;

        public Fixed266 Width => Fixed266.From((int)_GlyphMetrics.Width);
        public Fixed266 Height => Fixed266.From((int)_GlyphMetrics.Height);
    }
}
