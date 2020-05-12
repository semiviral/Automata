// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

#region

#endregion

#region

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#endregion

namespace Automata.Collections
{
    public class OctreeNode<T> where T : IEquatable<T>
    {
        #region Instance Members

        private OctreeNode<T>[]? _Nodes;

        public bool IsUniform => _Nodes == null;

        public T Value { get; private set; }

        public OctreeNode<T> this[int index]
        {
            get
            {
                if (_Nodes == null)
                {
                    throw new NullReferenceException(nameof(_Nodes));
                }

                return _Nodes[index];
            }
        }

        #endregion

        /// <summary>
        ///     Creates an in-memory compressed 3D representation of any unmanaged data type.
        /// </summary>
        /// <param name="value">Initial value of the collection.</param>
        public OctreeNode(T value) => Value = value;


        #region Data Operations

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public T GetPoint(int extent, int x, int y, int z)
        {
            if (IsUniform)
            {
                return Value;
            }

            Debug.Assert(_Nodes != null);

            Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

            return _Nodes[octant].GetPoint(extent >> 1, x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void SetPoint(int extent, int x, int y, int z, T newValue)
        {
            if (IsUniform)
            {
                if (Value.Equals(newValue))
                {
                    return;
                }
                else if (extent < 1)
                {
                    // reached smallest possible depth (usually 1x1x1) so
                    // set value and return
                    Value = newValue;
                    return;
                }
                else
                {
                    Populate();
                }
            }

            Debug.Assert(_Nodes != null);

            Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

            // recursively dig into octree and set
            _Nodes[octant].SetPoint(extent >> 1, x, y, z, newValue);

            // on each recursion back-step, ensure integrity of node
            // and collapse if all child node values are equal
            if (CheckShouldCollapse())
            {
                Collapse();
            }
        }

        #endregion


        #region Helper Methods

        private void Populate()
        {
            _Nodes = new[]
            {
                new OctreeNode<T>(Value),
                new OctreeNode<T>(Value),
                new OctreeNode<T>(Value),
                new OctreeNode<T>(Value),
                new OctreeNode<T>(Value),
                new OctreeNode<T>(Value),
                new OctreeNode<T>(Value),
                new OctreeNode<T>(Value)
            };
        }

        public void PopulateRecursive(float extent)
        {
            if (extent <= 1f)
            {
                return;
            }

            extent /= 2f;

            Populate();

            Debug.Assert(_Nodes != null);

            foreach (OctreeNode<T> octreeNode in _Nodes)
            {
                octreeNode.PopulateRecursive(extent);
            }
        }

        private void Collapse()
        {
            Debug.Assert(_Nodes != null);

            Value = _Nodes[0].Value;
            _Nodes = null;
        }

        private bool CheckShouldCollapse()
        {
            if (IsUniform)
            {
                return false;
            }

            Debug.Assert(_Nodes != null);

            T firstValue = _Nodes[0].Value;

            // avoiding using linq here for performance sensitivity
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (OctreeNode<T> octreeNode in _Nodes)
            {
                if (!octreeNode.IsUniform || !octreeNode.Value.Equals(firstValue))
                {
                    return false;
                }
            }

            return true;
        }

        public void CollapseRecursive()
        {
            if (IsUniform)
            {
                return;
            }

            Debug.Assert(_Nodes != null);

            foreach (OctreeNode<T> octreeNode in _Nodes)
            {
                octreeNode.CollapseRecursive();
            }

            if (CheckShouldCollapse())
            {
                Collapse();
            }
        }

        #endregion
    }
}
