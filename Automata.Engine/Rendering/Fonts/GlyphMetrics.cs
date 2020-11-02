using System;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.Fonts.FreeTypePrimitives;

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

        public Fixed266 Width => _GlyphMetrics.Width;
        public Fixed266 Height => _GlyphMetrics.Height;

        internal GlyphMetrics(IntPtr handle) => Handle = handle;
        internal GlyphMetrics(FreeTypeGlyphMetrics glyphMetrics) => _GlyphMetrics = glyphMetrics;
    }
}
