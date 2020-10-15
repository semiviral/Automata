namespace Automata.Engine.Numerics.Color
{
    public readonly partial struct RGBA32f
    {
        public static RGBA8 ToRGBA8(RGBA32f a) =>
            new RGBA8(
                (byte)(byte.MaxValue * a.R),
                (byte)(byte.MaxValue * a.G),
                (byte)(byte.MaxValue * a.B),
                (byte)(byte.MaxValue * a.A)
            );
    }
}
