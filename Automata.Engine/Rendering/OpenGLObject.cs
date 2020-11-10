using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering
{
    public abstract class OpenGLObject
    {
        protected readonly GL GL;

        public uint Handle { get; protected init; }

        public OpenGLObject(GL gl) => GL = gl;
    }
}
