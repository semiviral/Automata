using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class FenceSync : IDisposable
    {
        private readonly GL _GL;

        public nint Handle { get; }

        public FenceSync(GL gl, uint flags = 0u)
        {
            _GL = gl;
            Handle = gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, flags);
        }

        public void WaitGPU(ulong timeout, uint flags = 0u) => _GL.WaitSync(Handle, flags, timeout);
        public SyncStatus WaitCPU(ulong timeout, uint flags = 0u) => (SyncStatus)_GL.ClientWaitSync(Handle, flags, timeout);

        public void BusyWaitCPU()
        {
            while (true)
                switch (WaitCPU(1u, (uint)GLEnum.SyncFlushCommandsBit))
                {
                    case SyncStatus.AlreadySignaled:
                    case SyncStatus.ConditionSatisfied: return;
                }
        }

        public void Regenerate(uint flags = 0u)
        {
            _GL.DeleteSync((nint)Handle);
            _GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, flags);
        }


        #region IDisposable

        public void Dispose()
        {
            _GL.DeleteSync((nint)Handle);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
