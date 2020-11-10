using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Shaders
{
    public class ProgramPipeline : IEquatable<ProgramPipeline>, IDisposable
    {
        private readonly GL _GL;
        private readonly ShaderProgram _VertexShader;
        private readonly ShaderProgram _FragmentShader;

        public uint Handle { get; }

        public unsafe ProgramPipeline(GL gl, ShaderProgram vertexShader, ShaderProgram fragmentShader)
        {
            _GL = gl;
            _VertexShader = vertexShader;
            _FragmentShader = fragmentShader;

            uint handle = 0;
            _GL.CreateProgramPipelines(1, &handle);
            Handle = handle;

            _GL.UseProgramStages(Handle, (uint)UseProgramStageMask.VertexShaderBit, _VertexShader.Handle);
            _GL.UseProgramStages(Handle, (uint)UseProgramStageMask.FragmentShaderBit, _FragmentShader.Handle);
        }

        public ShaderProgram Stage(ShaderType shaderType) => shaderType switch
        {
            ShaderType.VertexShader => _VertexShader,
            ShaderType.FragmentShader => _FragmentShader,
            _ => throw new ArgumentOutOfRangeException(nameof(shaderType))
        };

        public void Bind() => _GL.BindProgramPipeline(Handle);
        public void Dispose() => _GL.DeleteProgramPipeline(Handle);

        public bool Equals(ProgramPipeline? other) => other is not null && (other.Handle == Handle);
        public override bool Equals(object? obj) => obj is ProgramPipeline other && Equals(other);

        public override int GetHashCode() => (int)Handle;

        public static bool operator ==(ProgramPipeline? left, ProgramPipeline? right) => Equals(left, right);
        public static bool operator !=(ProgramPipeline? left, ProgramPipeline? right) => !Equals(left, right);
    }
}
