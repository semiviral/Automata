#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class ShaderLoadException : Exception
    {
        private ShaderType Type { get; }
        private string InfoLog { get; }

        public ShaderLoadException(ShaderType type, string infoLog)
            : base($"Error compiling shader of type `{type}`: {infoLog}")

        {
            Type = type;
            InfoLog = infoLog;
        }
    }
}
