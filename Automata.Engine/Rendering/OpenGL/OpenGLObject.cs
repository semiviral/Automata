using System;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public abstract class OpenGLObject : IDisposable
    {
#if DEBUG
        private static readonly ConcurrentDictionary<uint, OpenGLObject> _ObjectsAlive = new ConcurrentDictionary<uint, OpenGLObject>();
        public static IReadOnlyDictionary<uint, OpenGLObject> ObjectsAlive => _ObjectsAlive;
        private readonly uint _RandomKey;
#endif

        protected readonly GL GL;

        public uint Handle { get; protected init; }
        public bool Disposed { get; private set; }

        public OpenGLObject(GL gl)
        {
#if DEBUG
            _RandomKey = (uint)RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
            _ObjectsAlive.TryAdd(_RandomKey, this);
#endif

            GL = gl;
        }


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

#if DEBUG
            _ObjectsAlive.TryRemove(_RandomKey, out _);
#endif
        }

        protected virtual void CleanupManagedResources() { }
        protected virtual void CleanupNativeResources() { }

        ~OpenGLObject() => Dispose(true);

        #endregion
    }
}
