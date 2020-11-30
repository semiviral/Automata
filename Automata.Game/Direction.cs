using System;
using System.Runtime.CompilerServices;
using Automata.Engine.Numerics;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

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
        East = 1 << 0,

        /// <summary>
        ///     Positive on Y axis
        /// </summary>
        Up = 1 << 1,

        /// <summary>
        ///     Positive on Z axis
        /// </summary>
        North = 1 << 2,

        /// <summary>
        ///     Negative on X axis
        /// </summary>
        West = 1 << 3,

        /// <summary>
        ///     Negative on Y axis
        /// </summary>
        Down = 1 << 4,

        /// <summary>
        ///     Negative on Z axis
        /// </summary>
        South = 1 << 5,

        Mask = byte.MaxValue
    }

    public static class Directions
    {
        public static Vector3<int> North { get; }
        public static Vector3<int> East { get; }
        public static Vector3<int> South { get; }
        public static Vector3<int> West { get; }
        public static Vector3<int> Up { get; }
        public static Vector3<int> Down { get; }

        public static Vector3<int>[] CardinalDirectionNormals { get; }
        public static Vector3<int>[] AllDirectionNormals { get; }

        static Directions()
        {
            North = new Vector3<int>(0, 0, 1);
            East = new Vector3<int>(1, 0, 0);
            South = new Vector3<int>(0, 0, -1);
            West = new Vector3<int>(-1, 0, 0);
            Up = new Vector3<int>(0, 1, 0);
            Down = new Vector3<int>(0, -1, 0);

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
