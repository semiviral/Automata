namespace Automata.Numerics.Color
{
    public readonly partial struct Color32
    {
        public static Color8 ToColor8(Color32 a) =>
            new Color8(
                (byte)(byte.MaxValue * a.R),
                (byte)(byte.MaxValue * a.G),
                (byte)(byte.MaxValue * a.B),
                (byte)(byte.MaxValue * a.A)
            );
    }
}
