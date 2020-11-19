using System;
using Automata.Engine.Components;
using Automata.Engine.Rendering.OpenGL.Buffers;

namespace Automata.Engine.Rendering.OpenGL
{
    public class DrawElementsIndirectAllocation<TIndex, TVertex> : ComponentChangeable, IEquatable<DrawElementsIndirectAllocation<TIndex, TVertex>>, IDisposable
        where TIndex : unmanaged
        where TVertex : unmanaged
    {
        private MeshArrayMemory<TIndex, TVertex>? _Allocation;

        public MeshArrayMemory<TIndex, TVertex>? Allocation
        {
            get => _Allocation;
            set
            {
                _Allocation = value;
                Changed = true;
            }
        }


        #region IEquatable

        public bool Equals(DrawElementsIndirectAllocation<TIndex, TVertex>? other) => other is not null && (_Allocation == other._Allocation);
        public override bool Equals(object? obj) => obj is DrawElementsIndirectAllocation<TIndex, TVertex> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode());

        public static bool operator ==(DrawElementsIndirectAllocation<TIndex, TVertex>? left, DrawElementsIndirectAllocation<TIndex, TVertex>? right) =>
            Equals(left, right);

        public static bool operator !=(DrawElementsIndirectAllocation<TIndex, TVertex>? left, DrawElementsIndirectAllocation<TIndex, TVertex>? right) =>
            !Equals(left, right);

        #endregion


        #region IDisposable

        public void Dispose()
        {
            Allocation?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}