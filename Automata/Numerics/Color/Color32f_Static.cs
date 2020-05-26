namespace Automata.Numerics.Color
{
    public readonly partial struct Color32f
    {
        public static Color8ui ToColor8(Color32f a) =>
            new Color8ui(
                (byte)(byte.MaxValue * a.R),
                (byte)(byte.MaxValue * a.G),
                (byte)(byte.MaxValue * a.B),
                (byte)(byte.MaxValue * a.A)
            );
    }
}
