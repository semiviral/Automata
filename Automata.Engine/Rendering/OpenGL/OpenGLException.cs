using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class OpenGLException : Exception
    {
        public GLEnum Error { get; }

        public OpenGLException(GLEnum error) => Error = error;

        public override string ToString() => $"OpenGL exception occurred: {Error}";
    }
}
