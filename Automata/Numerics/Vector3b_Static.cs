#region



// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace Automata.Numerics
{
    public readonly partial struct Vector3b
    {
        public static bool All(Vector3b a) => a.X && a.Y && a.Z;
        public static bool Any(Vector3b a) => a.X || a.Y || a.Z;
    }
}
