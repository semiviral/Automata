using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public abstract class OpenGLObject : IDisposable
    {
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
#if DEBUG
        private static readonly ConcurrentDictionary<uint, OpenGLObject> _ObjectsAlive = new();
        public static IReadOnlyDictionary<uint, OpenGLObject> ObjectsAlive => _ObjectsAlive;
        private readonly uint _RandomKey;
#endif


        #region IDisposable

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            DisposeInternal();

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(OpenGLObject)} 0x{Handle:x})", "OpenGL object disposed."));
            GC.SuppressFinalize(this);
            Disposed = true;

#if DEBUG
            _ObjectsAlive.TryRemove(_RandomKey, out _);
#endif
        }

        protected virtual void DisposeInternal() { }

        #endregion
    }
}
