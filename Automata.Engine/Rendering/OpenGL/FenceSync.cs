using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class FenceSync : OpenGLObject
    {
        public FenceSync(GL gl, uint flags = 0u) : base(gl) => Handle = (uint)GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, flags);

        public void WaitGPU(ulong timeout, uint flags = 0u) => GL.WaitSync((nint)Handle, flags, timeout);
        public SyncStatus WaitCPU(ulong timeout, uint flags = 0u) => (SyncStatus)GL.ClientWaitSync((nint)Handle, flags, timeout);

        public void BusyWaitCPU()
        {
            while (true)
                switch ((SyncStatus)GL.ClientWaitSync((nint)Handle, (uint)GLEnum.SyncFlushCommandsBit, 1))
                {
                    case SyncStatus.AlreadySignaled:
                    case SyncStatus.ConditionSatisfied: return;
                }
        }

        public void Regenerate(uint flags = 0u)
        {
            GL.DeleteSync((nint)Handle);
            GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, flags);
        }


        #region IDisposable

        protected override void DisposeInternal() => GL.DeleteSync((nint)Handle);

        #endregion
    }
}
