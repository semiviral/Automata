using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering
{
    public class OpenGLObject
    {
        protected readonly GL GL;

        public uint Handle { get; protected init; }

        public OpenGLObject(GL gl) => GL = gl;
    }
}
