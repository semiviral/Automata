using System;
using Automata.Engine.Components;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class DrawIndirectAllocation<TIndex, TVertex> : ComponentChangeable, IDisposable
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private AllocationWrapper<TIndex, TVertex>? _Allocation;

        public AllocationWrapper<TIndex, TVertex>? Allocation
        {
            get => _Allocation;
            set
            {
                _Allocation = value;
                Changed = true;
            }
        }

        public void Dispose()
        {
            Allocation?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
