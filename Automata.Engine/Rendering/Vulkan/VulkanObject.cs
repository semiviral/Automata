using System;
using Automata.Engine.Rendering.OpenGL;
using Serilog;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public abstract class VulkanObject : IDisposable
    {
        protected readonly Vk VK;

        public nint Handle { get; protected init; }
        public bool Disposed { get; private set; }

        protected VulkanObject(Vk vk) => VK = vk;


        #region IDisposable

        public void Dispose()
        {
            Dispose(false);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(OpenGLObject)} 0x{Handle:x})", "OpenGL object disposed."));
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool finalizer)
        {
            if (Disposed)
            {
                return;
            }

            if (!finalizer)
            {
                CleanupManagedResources();
            }

            CleanupNativeResources();
            Disposed = true;
        }

        protected virtual void CleanupManagedResources() { }
        protected virtual void CleanupNativeResources() { }

        ~VulkanObject() => Dispose(true);

        #endregion
    }
}
