#region

using System;

#endregion

namespace Automata.Engine.Rendering.OpenGL
{
    public class ShaderProgramException : Exception
    {
        public string InfoLog { get; }

        public ShaderProgramException(string infoLog)
            : base($"Shader program failed to link: {infoLog}") =>
            InfoLog = infoLog;
    }
}
