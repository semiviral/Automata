using System;
using Automata.Engine.Components;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class DrawIndirectAllocation : ComponentChangeable, IDisposable
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

        public void Dispose()
        {
            Allocation?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
