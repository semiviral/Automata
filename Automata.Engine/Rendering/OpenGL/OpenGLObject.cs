using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public abstract class OpenGLObject : IDisposable
    {
        protected readonly GL GL;

        public uint Handle { get; protected init; }
        public bool Disposed { get; private set; }

        public OpenGLObject(GL gl) => GL = gl;


        #region IDisposable

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            DisposeInternal();

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        protected virtual void DisposeInternal() { }

        #endregion
    }
}
