using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class ShaderStorageBufferObject : OpenGLObject, IDisposable
    {
        public ShaderStorageBufferObject(GL gl) : base(gl)
        {
            Handle = GL.CreateBuffer();
        }
    }
}
