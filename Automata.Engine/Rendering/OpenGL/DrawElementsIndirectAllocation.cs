using System;
using Automata.Engine.Rendering.OpenGL.Buffers;

namespace Automata.Engine.Rendering.OpenGL
{
    public class DrawElementsIndirectAllocation<TIndex, TVertex> : Component, IEquatable<DrawElementsIndirectAllocation<TIndex, TVertex>>, IDisposable
        where TIndex : unmanaged
        where TVertex : unmanaged
    {
        public MeshMemory<TIndex, TVertex>? Allocation { get; set; }


        #region IEquatable

        public bool Equals(DrawElementsIndirectAllocation<TIndex, TVertex>? other) => other is not null && (Allocation == other.Allocation);
        public override bool Equals(object? obj) => obj is DrawElementsIndirectAllocation<TIndex, TVertex> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode());

        public static bool operator ==(DrawElementsIndirectAllocation<TIndex, TVertex>? left, DrawElementsIndirectAllocation<TIndex, TVertex>? right) =>
            Equals(left, right);

        public static bool operator !=(DrawElementsIndirectAllocation<TIndex, TVertex>? left, DrawElementsIndirectAllocation<TIndex, TVertex>? right) =>
            !Equals(left, right);

        #endregion


        #region IDisposable

        protected override void CleanupManagedResources() => Allocation?.Dispose();

        #endregion
    }
}
