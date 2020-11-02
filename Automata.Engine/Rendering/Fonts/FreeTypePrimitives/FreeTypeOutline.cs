using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FreeTypeOutline
    {
        internal readonly short NContours;
        internal readonly short NPoints;

        internal readonly IntPtr Points;
        internal readonly IntPtr Tags;
        internal readonly IntPtr Contours;

        internal readonly OutlineFlags Flags;
    }
}
