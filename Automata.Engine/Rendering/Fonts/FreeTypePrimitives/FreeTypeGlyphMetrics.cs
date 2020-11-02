using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FreeTypeGlyphMetrics
    {
        internal readonly Fixed266 Width;
        internal readonly Fixed266 Height;

        internal readonly Fixed266 HorzionBearingX;
        internal readonly Fixed266 HorizonBearingY;
        internal readonly Fixed266 HorizonAdvance;

        internal readonly Fixed266 VerticalBearingX;
        internal readonly Fixed266 VerticalBearingY;
        internal readonly Fixed266 VerticalAdvance;
    }
}
