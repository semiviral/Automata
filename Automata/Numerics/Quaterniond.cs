// ReSharper disable ConvertToAutoProperty

namespace Automata.Numerics
{
    public readonly struct Quaterniond
    {
        public static Quaterniond Identity { get; } = new Quaterniond(0d, 0d, 0d, 1d);

        private readonly Vector3d _Vector;
        private readonly double _Rotation;

        public double X => _Vector.X;
        public double Y => _Vector.Y;
        public double Z => _Vector.Z;
        public double W => _Rotation;

        public Quaterniond(double x, double y, double z, double w) => (_Vector, _Rotation) = (new Vector3d(x, y, z), w);
    }
}
