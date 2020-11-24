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
            {
                switch (WaitCPU(1u, (uint)GLEnum.SyncFlushCommandsBit))
                {
                    case SyncStatus.AlreadySignaled:
                    case SyncStatus.ConditionSatisfied: return;
                }
            }
        }


        #region IDisposable

        protected override void CleanupNativeResources() => GL.DeleteSync((nint)Handle);

        #endregion
    }
}
