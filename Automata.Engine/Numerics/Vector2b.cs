#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector2b
    {
        #region Fields / Properties

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

        #endregion


        #region Constructors

        private Vector2b(byte x, byte y) => (_X, _Y) = (x, y);

        public Vector2b(bool value)
        {
            byte numericValue = value.AsByte();

            (_X, _Y) = (numericValue, numericValue);
        }

        public Vector2b(bool x, bool y) => (_X, _Y) = (x.AsByte(), y.AsByte());

        #endregion


        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is Vector2b a)
            {
                return All(a == this);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => _X.GetHashCode() ^ _Y.GetHashCode();

        public override string ToString() => string.Format(FormatHelper.VECTOR_2_COMPONENT, nameof(Vector2b), _X, _Y);

        #endregion


        #region Operators

        public static Vector2b operator ==(Vector2b a, Vector2b b) => EqualsImpl(a, b);
        public static Vector2b operator !=(Vector2b a, Vector2b b) => NotEqualsImpl(a, b);
        public static Vector2b operator !(Vector2b a) => new Vector2b((byte)~a._X, (byte)~a._Y);

        #endregion


        #region Conversions

        public static unsafe explicit operator Vector128<byte>(Vector2b a) => Sse2.LoadVector128(&a._X);
        public static unsafe explicit operator Vector2b(Vector128<byte> a) => *(Vector2b*)&a;

        public static explicit operator Vector2b(Vector128<int> a) => new Vector2b(
            (byte)a.GetElement(0),
            (byte)a.GetElement(1));

        #endregion
    }
}
