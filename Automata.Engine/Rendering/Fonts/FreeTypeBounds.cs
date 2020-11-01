using System;
using System.Runtime.InteropServices;

using FreeTypeLong = System.IntPtr;
using FreeTypeULong = System.UIntPtr;

namespace Automata.Engine.Rendering.Fonts
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FreeTypeBounds : IEquatable<FreeTypeBounds>
    {
        private FreeTypeLong XMin, YMin;
        private FreeTypeLong XMax, YMax;

        public int Left => (int)XMin;
        public int Bottom => (int)YMin;
        public int Right => (int)XMax;
        public int Top => (int)YMax;

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
