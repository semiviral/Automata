using System;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class RingFenceSync : IDisposable
    {
        private readonly GL _GL;
        private readonly Ring _Ring;
        private readonly FenceSync?[] _RingSyncs;

        public nuint Current => _Ring.Current;

        public RingFenceSync(GL gl, nuint count)
        {
            _GL = gl;
            _Ring = new Ring(count);
            _RingSyncs = new FenceSync?[count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitEnterNext()
        {
            // wait to enter next ring, then increment to it
            _RingSyncs[(int)_Ring.NextRing()]?.BusyWaitCPU();
            _Ring.Increment();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FenceCurrent()
        {
            // create fence for current ring
            _RingSyncs[(int)_Ring.Current]?.Dispose();
            _RingSyncs[(int)_Ring.Current] = new FenceSync(_GL);
        }


        #region IDisposable

        public void Dispose()
        {
            foreach (FenceSync? fenceSync in _RingSyncs)
            {
                fenceSync?.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
