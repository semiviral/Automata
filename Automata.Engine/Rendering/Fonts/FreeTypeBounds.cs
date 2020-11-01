using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FreeTypeBounds : IEquatable<FreeTypeBounds>
    {
        private readonly long XMin, YMin;
        private readonly long XMax, YMax;

        public long Left => XMin;
        public long Bottom => YMin;
        public long Right => XMax;
        public long Top => YMax;

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
