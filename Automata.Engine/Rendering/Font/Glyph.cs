using Automata.Engine.Numerics;

namespace Automata.Engine.Rendering.Font
{
    public class Glyph
    {
        public uint Handle { get; }
        public Vector2i Size { get; }
        public Vector2i Bearing { get; }
        public uint Advance { get; }
    }
}
