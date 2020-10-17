#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

#endregion

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector2b : IEquatable<Vector2b>
    {
        public static Vector2b True { get; } = new Vector2b(true);
        public static Vector2b False { get; } = new Vector2b(false);

        private readonly byte _X;
        private readonly byte _Y;

        public bool X => _X.AsBool();
        public bool Y => _Y.AsBool();

        public bool this[int index] => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        private Vector2b(byte x, byte y) => (_X, _Y) = (x, y);

        public Vector2b(bool value)
        {
            byte numericValue = value.AsByte();

            (_X, _Y) = (numericValue, numericValue);
        }

        public Vector2b(bool x, bool y) => (_X, _Y) = (x.AsByte(), y.AsByte());

        #region Overrides

        public override bool Equals(object? obj) => obj is Vector2b other && Equals(other);
        public bool Equals(Vector2b other) => All(this == other);

        public override int GetHashCode() => _X.GetHashCode() ^ _Y.GetHashCode();

        public override string ToString() => string.Format(FormatHelper.VECTOR_2_COMPONENT, nameof(Vector2b), X, Y);

        #endregion

        #region Operators

        public static Vector2b operator ==(Vector2b a, Vector2b b) => EqualsImpl(a, b);
        public static Vector2b operator !=(Vector2b a, Vector2b b) => NotEqualsImpl(a, b);
        public static Vector2b operator !(Vector2b a) => new Vector2b((byte)~a._X, (byte)~a._Y);

        #endregion

        #region Conversions

        public static explicit operator Vector128<byte>(Vector2b a) => Unsafe.As<Vector2b, Vector128<byte>>(ref a);
        public static explicit operator Vector2b(Vector128<byte> a) => Unsafe.As<Vector128<byte>, Vector2b>(ref a);

        public static explicit operator Vector2b(Vector128<int> a) => new Vector2b(
            (byte)a.GetElement(0),
            (byte)a.GetElement(1));

        #endregion
    }
}
