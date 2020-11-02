using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FreeTypeGlyphMetrics
    {
        internal readonly long Width;
        internal readonly long Height;

        internal readonly long HorzionBearingX;
        internal readonly long HorizonBearingY;
        internal readonly long HorizonAdvance;

        internal readonly long VerticalBearingX;
        internal readonly long VerticalBearingY;
        internal readonly long VerticalAdvance;
    }
}
