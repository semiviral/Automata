#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector3b
    {
        #region Fields / Properties

        private static readonly string _ToStringFormat = $"{typeof(Vector3b)}({{0}}, {{1}}, {{2}})";

        private readonly byte _X;
        private readonly byte _Y;
        private readonly byte _Z;

        public bool X => _X.AsBool();
        public bool Y => _Y.AsBool();
        public bool Z => _Z.AsBool();

        public bool this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #endregion


        #region Constructors

        private Vector3b(byte x, byte y, byte z) => (_X, _Y, _Z) = (x, y, z);

        public Vector3b(bool value)
        {
            byte numericValue = value.AsByte();

            (_X, _Y, _Z) = (numericValue, numericValue, numericValue);
        }

        public Vector3b(bool x, bool y, bool z) =>
            (_X, _Y, _Z) = (x.AsByte(), y.AsByte(), z.AsByte());

        #endregion


        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is Vector3b a)
            {
                return All(a == this);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => _X.GetHashCode() ^ _Y.GetHashCode() ^ _Z.GetHashCode();

        public override string ToString() => string.Format(_ToStringFormat, X, Y, Z);

        #endregion


        #region Operators

        public static Vector3b operator ==(Vector3b a, Vector3b b) => EqualsImpl(a, b);
        public static Vector3b operator !=(Vector3b a, Vector3b b) => NotEqualsImpl(a, b);
        public static Vector3b operator !(Vector3b a) => new Vector3b((byte)~a._X, (byte)~a._Y, (byte)~a._Z);
        public static Vector3b operator |(Vector3b a, Vector3b b) => OrImpl(a, b);

        #endregion


        #region Conversions

        public static unsafe explicit operator Vector128<byte>(Vector3b a) => Sse2.LoadVector128(&a._X);

        public static unsafe explicit operator Vector3b(Vector128<byte> a) => *(Vector3b*)&a;

        public static explicit operator Vector3b(Vector128<int> a) => new Vector3b(
            (byte)a.GetElement(0),
            (byte)a.GetElement(1),
            (byte)a.GetElement(2));

        public static explicit operator Vector3b(Vector256<double> a) => new Vector3b(
            a.GetElement(0).FirstByte(),
            a.GetElement(1).FirstByte(),
            a.GetElement(2).FirstByte());

        #endregion
    }
}
