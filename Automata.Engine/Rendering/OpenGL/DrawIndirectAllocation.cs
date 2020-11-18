using System;
using Automata.Engine.Components;

namespace Automata.Engine.Rendering.OpenGL
{
    public class DrawIndirectAllocation<TIndex, TVertex> : ComponentChangeable, IEquatable<DrawIndirectAllocation<TIndex, TVertex>>, IDisposable
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private AllocationWrapper? _Allocation;

        public AllocationWrapper? Allocation
        {
            get => _Allocation;
            set
            {
                _Allocation = value;
                Changed = true;
            }
        }


        #region IEquatable

        public bool Equals(DrawIndirectAllocation<TIndex, TVertex>? other) => other is not null && (_Allocation == other._Allocation);
        public override bool Equals(object? obj) => obj is DrawIndirectAllocation<TIndex, TVertex> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _Allocation);

        public static bool operator ==(DrawIndirectAllocation<TIndex, TVertex>? left, DrawIndirectAllocation<TIndex, TVertex>? right) => Equals(left, right);
        public static bool operator !=(DrawIndirectAllocation<TIndex, TVertex>? left, DrawIndirectAllocation<TIndex, TVertex>? right) => !Equals(left, right);

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
