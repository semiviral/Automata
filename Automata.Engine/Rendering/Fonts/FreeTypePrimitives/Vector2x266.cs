using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vector2x266
    {
        private readonly IntPtr _X;
        private readonly IntPtr _Y;

        public Fixed266 X() => Fixed266.From((int)_X);
        public Fixed266 Y() => Fixed266.From((int)_Y);
    }
}
