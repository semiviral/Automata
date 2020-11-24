using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Shaders
{
    public class ProgramPipeline : OpenGLObject, IEquatable<ProgramPipeline>
    {
        private readonly ShaderProgram _VertexShader;
        private readonly ShaderProgram _FragmentShader;

        public ProgramPipeline(GL gl, ShaderProgram vertexShader, ShaderProgram fragmentShader) : base(gl)
        {
            _VertexShader = vertexShader;
            _FragmentShader = fragmentShader;

            Handle = GL.CreateProgramPipeline();
            GL.UseProgramStages(Handle, (uint)UseProgramStageMask.VertexShaderBit, _VertexShader.Handle);
            GL.UseProgramStages(Handle, (uint)UseProgramStageMask.FragmentShaderBit, _FragmentShader.Handle);

            CheckInfoLogAndThrow();
        }

        public ShaderProgram Stage(ShaderType shaderType) => shaderType switch
        {
            ShaderType.VertexShader => _VertexShader,
            ShaderType.FragmentShader => _FragmentShader,
            _ => throw new ArgumentOutOfRangeException(nameof(shaderType))
        };

        [SkipLocalsInit]
        public unsafe bool TryGetInfoLog([NotNullWhen(true)] out string? infoLog)
        {
            GL.GetProgramPipeline(Handle, PipelineParameterName.InfoLogLength, out int infoLogLength);

            if (infoLogLength is 0)
            {
                infoLog = string.Empty;
                return false;
            }

            Span<byte> infoLogSpan = stackalloc byte[infoLogLength];
            GL.GetProgramPipelineInfoLog(Handle, (uint)infoLogLength, (uint*)&infoLogLength, infoLogSpan);
            infoLog = Encoding.ASCII.GetString(infoLogSpan);
            return true;
        }

        private void CheckInfoLogAndThrow()
        {
            if (TryGetInfoLog(out string? infoLog))
            {
                throw new ShaderLoadException((ShaderType)0, infoLog);
            }
        }


        #region Binding

        public void Bind() => GL.BindProgramPipeline(Handle);

        #endregion


        #region IEquatable

        public bool Equals(ProgramPipeline? other) => other is not null && (other.Handle == Handle);
        public override bool Equals(object? obj) => obj is ProgramPipeline other && Equals(other);

        public override int GetHashCode() => (int)Handle;

        public static bool operator ==(ProgramPipeline? left, ProgramPipeline? right) => Equals(left, right);
        public static bool operator !=(ProgramPipeline? left, ProgramPipeline? right) => !Equals(left, right);

        #endregion


        #region IDisposable

        protected override void CleanupNativeResources() => GL.DeleteProgramPipeline(Handle);

        #endregion
    }
}
