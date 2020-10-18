#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Automata.Engine.Numerics;

#endregion

namespace Automata.Engine.Collections
{
    public class Octree<T> : INodeCollection<T> where T : IEquatable<T>
    {
        private readonly int _Extent;
        private readonly OctreeNode<T> _RootNode;

        public Octree(int size, T initialValue, bool fullyPopulate)
        {
            if ((size <= 0) || ((size & (size - 1)) != 0))
            {
                throw new ArgumentException($"Size must be a power of two ({size}).", nameof(size));
            }

            _Extent = size >> 1;
            _RootNode = new OctreeNode<T>(initialValue);

            Length = (int)Math.Pow(size, 3);

            if (fullyPopulate)
            {
                // todo
                _RootNode.PopulateRecursive(_Extent);
            }
        }

        public T Value => _RootNode.Value;
        public bool IsUniform => _RootNode.IsUniform;

        public int Length { get; }


        public IEnumerable<T> GetAllData()
        {
            int size = _Extent << 1;

            for (int index = 0; index < Length; index++)
            {
                yield return GetPoint(Vector3i.Project3D(index, size));
            }
        }

        public void CopyTo(T[] destinationArray)
        {
            if (destinationArray.Rank != 1)
            {
                throw new RankException("Only single dimension arrays are supported here.");
            }
            else if (destinationArray.Length < Length)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationArray), "Destination array was not long enough.");
            }

            int size = _Extent << 1;

            for (int index = 0; index < destinationArray.Length; index++)
            {
                destinationArray[index] = GetPoint(Vector3i.Project3D(index, size));
            }
        }

        #region GetPoint

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetPoint(Vector3i point) => GetPointIterative(point.X, point.Y, point.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetPoint(int x, int y, int z) => GetPointIterative(x, y, z);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private T GetPointIterative(int x, int y, int z)
        {
            OctreeNode<T> currentNode = _RootNode;

            for (int extent = _Extent; !currentNode.IsUniform; extent /= 2)
            {
                Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

                currentNode = currentNode[octant];
            }

            return currentNode.Value;
        }

        #endregion

        #region SetPoint

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPoint(Vector3i point, T value) => _RootNode.SetPoint(_Extent, point.X, point.Y, point.Z, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPoint(int x, int y, int z, T value) => _RootNode.SetPoint(_Extent, x, y, z, value);

        #endregion
    }

    public static class Octree
    {
        // indexes:
        // bottom half quadrant indexes:
        // 1 3
        // 0 2
        // top half quadrant indexes:
        // 5 7
        // 4 6
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DetermineOctant(int extent, ref int x, ref int y, ref int z, out int octant)
        {
            octant = 0;

            if (x >= extent)
            {
                x -= extent;
                octant += 1;
            }

            if (y >= extent)
            {
                y -= extent;
                octant += 4;
            }

            if (z >= extent)
            {
                z -= extent;
                octant += 2;
            }
        }
    }
}
