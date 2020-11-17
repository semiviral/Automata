using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Shaders
{
    public class ProgramPipeline : OpenGLObject, IEquatable<ProgramPipeline>, IDisposable
    {
        private readonly ShaderProgram _VertexShader;
        private readonly ShaderProgram _FragmentShader;

        public unsafe ProgramPipeline(GL gl, ShaderProgram vertexShader, ShaderProgram fragmentShader) : base(gl)
        {
            _VertexShader = vertexShader;
            _FragmentShader = fragmentShader;

            uint handle = 0;
            GL.CreateProgramPipelines(1, &handle);
            Handle = handle;

            GL.UseProgramStages(Handle, (uint)UseProgramStageMask.VertexShaderBit, _VertexShader.Handle);
            GL.UseProgramStages(Handle, (uint)UseProgramStageMask.FragmentShaderBit, _FragmentShader.Handle);

            CheckShaderInfoLogAndThrow();
        }

        public ShaderProgram Stage(ShaderType shaderType) => shaderType switch
        {
            ShaderType.VertexShader => _VertexShader,
            ShaderType.FragmentShader => _FragmentShader,
            _ => throw new ArgumentOutOfRangeException(nameof(shaderType))
        };

        private void CheckShaderInfoLogAndThrow()
        {
            GL.GetProgramPipelineInfoLog(Handle, 2048u, out _, out string infoLog);

            if (!string.IsNullOrWhiteSpace(infoLog)) throw new ShaderLoadException((ShaderType)0, infoLog);
        }

        public void Bind() => GL.BindProgramPipeline(Handle);

        public void Dispose()
        {
            GL.DeleteProgramPipeline(Handle);
            GC.SuppressFinalize(this);
        }

        public bool Equals(ProgramPipeline? other) => other is not null && (other.Handle == Handle);
        public override bool Equals(object? obj) => obj is ProgramPipeline other && Equals(other);

        public override int GetHashCode() => (int)Handle;

        public static bool operator ==(ProgramPipeline? left, ProgramPipeline? right) => Equals(left, right);
        public static bool operator !=(ProgramPipeline? left, ProgramPipeline? right) => !Equals(left, right);
    }
}
