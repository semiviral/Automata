#region

using System;
using System.Runtime.CompilerServices;
using Automata.Engine.Numerics;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

#endregion


namespace Automata.Game
{
    /// <summary>
    ///     6-way direction in byte values.
    /// </summary>
    [Flags]
    public enum Direction : byte
    {
        /// <summary>
        ///     Positive on X axis
        /// </summary>
        East = 0b0000_0001,

        /// <summary>
        ///     Positive on Y axis
        /// </summary>
        Up = 0b0000_0010,

        /// <summary>
        ///     Positive on Z axis
        /// </summary>
        North = 0b0000_0100,

        /// <summary>
        ///     Negative on X axis
        /// </summary>
        West = 0b0000_1000,

        /// <summary>
        ///     Negative on Y axis
        /// </summary>
        Down = 0b0001_0000,

        /// <summary>
        ///     Negative on Z axis
        /// </summary>
        South = 0b0010_0000,

        Mask = 0b0011_1111
    }

    public static class Directions
    {
        public static Vector3i North { get; }
        public static Vector3i East { get; }
        public static Vector3i South { get; }
        public static Vector3i West { get; }
        public static Vector3i Up { get; }
        public static Vector3i Down { get; }

        public static Vector3i[] CardinalDirectionNormals { get; }
        public static Vector3i[] AllDirectionNormals { get; }

        static Directions()
        {
            North = new Vector3i(0, 0, 1);
            East = new Vector3i(1, 0, 0);
            South = new Vector3i(0, 0, -1);
            West = new Vector3i(-1, 0, 0);
            Up = new Vector3i(0, 1, 0);
            Down = new Vector3i(0, -1, 0);

            CardinalDirectionNormals = new[]
            {
                North,
                East,
                South,
                West
            };

            AllDirectionNormals = new[]
            {
                North,
                East,
                South,
                West,
                Up,
                Down
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAny(this Direction direction) => direction > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAll(this Direction direction) => (direction & Direction.Mask) is Direction.Mask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasDirection(this Direction direction, Direction target) => (direction & target) == target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction WithDirection(this Direction direction, Direction target) => direction | target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction WithoutDirection(this Direction direction, Direction target) => direction & ~target;
    }
}
