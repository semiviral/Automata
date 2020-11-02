using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FreeTypeBounds : IEquatable<FreeTypeBounds>
    {
        private readonly int XMin, YMin;
        private readonly int XMax, YMax;

        public int Left => XMin;
        public int Bottom => YMin;
        public int Right => XMax;
        public int Top => YMax;

        public bool Equals(FreeTypeBounds other) =>
            XMin.Equals(other.XMin)
            && YMin.Equals(other.YMin)
            && XMax.Equals(other.XMax)
            && YMax.Equals(other.YMax);

        public override bool Equals(object? obj) => obj is FreeTypeBounds other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(XMin, YMin, XMax, YMax);

        public static bool operator ==(FreeTypeBounds left, FreeTypeBounds right) => left.Equals(right);
        public static bool operator !=(FreeTypeBounds left, FreeTypeBounds right) => !left.Equals(right);
    }
}
