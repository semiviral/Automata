namespace Automata.Numerics
{
    public readonly partial struct Vector2b
    {
        public static bool All(Vector2b a) => a.X && a.Y;
        public static bool Any(Vector2b a) => a.X || a.Y;
    }
}
