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
        private class OctreeNode<TNode> where TNode : IEquatable<TNode>
        {
            private OctreeNode<TNode>[]? _Nodes;

            public bool IsUniform => _Nodes == null;

            public TNode Value { get; private set; }

            public OctreeNode<TNode>? this[int index] => _Nodes?[index];

            /// <summary>
            ///     Creates an in-memory compressed 3D representation of any unmanaged data type.
            /// </summary>
            /// <param name="value">Initial value of the collection.</param>
            public OctreeNode(TNode value) => Value = value;

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public TNode GetPoint(int extent, int x, int y, int z)
            {
                if (IsUniform) return Value;

                Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

                return _Nodes![octant].GetPoint(extent >> 1, x, y, z);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void SetPoint(int extent, int x, int y, int z, TNode newValue)
            {
                if (IsUniform)
                {
                    if (Value.Equals(newValue)) return;
                    else if (extent < 1)
                    {
                        // reached smallest possible depth (usually 1x1x1) so
                        // set value and return
                        Value = newValue;
                        return;
                    }
                    else Populate();
                }

                Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

                // recursively dig into octree and set
                _Nodes![octant].SetPoint(extent >> 1, x, y, z, newValue);

                // on each recursion back-step, ensure integrity of node
                // and collapse if all child node values are equal
                if (CheckShouldCollapse()) Collapse();
            }

            private void Populate()
            {
                _Nodes = new[]
                {
                    new OctreeNode<TNode>(Value),
                    new OctreeNode<TNode>(Value),
                    new OctreeNode<TNode>(Value),
                    new OctreeNode<TNode>(Value),
                    new OctreeNode<TNode>(Value),
                    new OctreeNode<TNode>(Value),
                    new OctreeNode<TNode>(Value),
                    new OctreeNode<TNode>(Value)
                };
            }

            private void Collapse()
            {
                Value = _Nodes![0].Value;
                _Nodes = null;
            }

            private bool CheckShouldCollapse()
            {
                if (IsUniform) return false;

                TNode firstValue = _Nodes![0].Value;

                // avoiding using linq here for performance sensitivity
                foreach (OctreeNode<TNode> octreeNode in _Nodes)
                {
                    if (!octreeNode.IsUniform || !octreeNode.Value.Equals(firstValue)) return false;
                }

                return true;
            }
        }

        private readonly int _Extent;
        private readonly OctreeNode<T> _RootNode;

        public Octree(int size, T initialValue)
        {
            if ((size <= 0) || ((size & (size - 1)) != 0)) throw new ArgumentException($"Size must be a power of two ({size}).", nameof(size));

            _Extent = size >> 1;
            _RootNode = new OctreeNode<T>(initialValue);

            Length = (int)Math.Pow(size, 3);
        }

        public T Value => _RootNode.Value;
        public bool IsUniform => _RootNode.IsUniform;

        public int Length { get; }

        public IEnumerable<T> GetAllData()
        {
            int size = _Extent << 1;

            for (int index = 0; index < Length; index++) yield return GetPoint(Vector3i.Project3D(index, size));
        }

        public void CopyTo(T[] destinationArray)
        {
            if (destinationArray.Rank != 1) throw new RankException("Only single dimension arrays are supported here.");
            else if (destinationArray.Length < Length)
                throw new ArgumentOutOfRangeException(nameof(destinationArray), "Destination array was not long enough.");

            int size = _Extent << 1;

            for (int index = 0; index < destinationArray.Length; index++) destinationArray[index] = GetPoint(Vector3i.Project3D(index, size));
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

            for (int extent = _Extent; !currentNode!.IsUniform; extent /= 2)
            {
                Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

                currentNode = currentNode[octant]!;
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
