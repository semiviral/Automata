using Automata.Engine.Numerics;

namespace Automata.Engine.Rendering.Fonts
{
    public class Glyph
    {
        public uint Handle { get; }
        public Vector2i Size { get; }
        public Vector2i Bearing { get; }
        public uint Advance { get; }
    }
}
