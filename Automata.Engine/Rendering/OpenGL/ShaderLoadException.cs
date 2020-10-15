#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Engine.Rendering.OpenGL
{
    public class ShaderLoadException : Exception
    {
        public ShaderType Type { get; }
        public string InfoLog { get; }

        public ShaderLoadException(ShaderType type, string infoLog) : base($"Error compiling shader of type '{type}': {infoLog}")
        {
            Type = type;
            InfoLog = infoLog;
        }
    }
}
