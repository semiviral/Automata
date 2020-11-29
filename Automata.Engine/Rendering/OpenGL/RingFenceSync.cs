using System;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class RingFenceSync : IDisposable
    {
        private readonly GL _GL;
        private readonly RingIncrementer _RingIncrementer;
        private readonly FenceSync?[] _RingSyncs;

        public nuint Current => _RingIncrementer.Current;

        public RingFenceSync(GL gl, nuint count)
        {
            _GL = gl;
            _RingIncrementer = new RingIncrementer(count);
            _RingSyncs = new FenceSync?[count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitCurrent() => _RingSyncs[(int)_RingIncrementer.Current]?.BusyWaitCPU();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitEnterNext()
        {
            // wait to enter next ring, then increment to it
            _RingSyncs[(int)_RingIncrementer.NextRing()]?.BusyWaitCPU();
            _RingIncrementer.Increment();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FenceCurrent()
        {
            // create fence for current ring
            _RingSyncs[(int)_RingIncrementer.Current]?.Dispose();
            _RingSyncs[(int)_RingIncrementer.Current] = new FenceSync(_GL);
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
