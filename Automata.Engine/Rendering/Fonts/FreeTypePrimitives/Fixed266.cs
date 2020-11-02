using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Fixed266
    {
        private readonly int _Value;

        public double Value => _Value / 64d;

        public Fixed266(int value) => _Value = value;

        public static Fixed266 From(int value) => new Fixed266(value);
    }
}
