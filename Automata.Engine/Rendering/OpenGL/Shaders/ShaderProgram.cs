using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Shaders
{
    public class ShaderProgram : IEquatable<ShaderProgram>, IDisposable
    {
        private static readonly string[] _ReservedUniformNames =
        {
            ProgramRegistry.RESERVED_UNIFORM_NAME_MATRIX_MVP,
            ProgramRegistry.RESERVED_UNIFORM_NAME_MATRIX_WORLD,
            ProgramRegistry.RESERVED_UNIFORM_NAME_MATRIX_OBJECT,
            ProgramRegistry.RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION,
            ProgramRegistry.RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS,
            ProgramRegistry.RESERVED_UNIFORM_NAME_VEC4_VIEWPORT
        };

        private readonly GL _GL;
        private readonly Dictionary<string, int> _CachedUniforms;

        public uint Handle { get; }
        public ShaderType Type { get; }
        public bool HasAutomataUniforms { get; }

        public ShaderProgram(GL gl, ShaderType shaderType, string path)
        {
            _GL = gl;
            _CachedUniforms = new Dictionary<string, int>();
            Type = shaderType;

            // this is kinda dumb, right?
            string[] shader =
            {
                File.ReadAllText(path)
            };

            Handle = _GL.CreateShaderProgram(Type, 1, shader);
            CheckShaderInfoLogAndThrow();

            CacheUniforms();
            HasAutomataUniforms = _CachedUniforms.Keys.Intersect(_ReservedUniformNames).Any();

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ShaderProgram), $"Loaded ({Type}): {path}"));
        }

        private void CacheUniforms()
        {
            _GL.GetProgram(Handle, ProgramPropertyARB.ActiveUniforms, out int uniformCount);

            for (uint uniformIndex = 0; uniformIndex < uniformCount; uniformIndex++)
            {
                string name = _GL.GetActiveUniform(Handle, uniformIndex, out _, out _);
                int location = _GL.GetUniformLocation(Handle, name);
                _CachedUniforms.Add(name, location);
            }
        }

        private void CheckShaderInfoLogAndThrow()
        {
            string infoLog = _GL.GetProgramInfoLog(Handle);

            if (!string.IsNullOrWhiteSpace(infoLog)) throw new ShaderLoadException(Type, infoLog);
        }

        public void BindUniformBuffer(UniformBuffer uniformBuffer)
        {

        }

        public bool TrySetUniform(string name, int value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform1(Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, float value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform1(Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, Vector3 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform3(Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, Vector4 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform4(Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, Matrix4x4 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniformMatrix4(Handle, location, 1, false, value.Unroll());
                return true;
            }
            else return false;
        }

        private bool TryGetUniformLocation(string name, out int location) => _CachedUniforms.TryGetValue(name, out location);

        public void Dispose() => _GL.DeleteProgram(Handle);

        public bool Equals(ShaderProgram? other) => other is not null && (other.Handle == Handle);
        public override bool Equals(object? obj) => obj is ShaderProgram other && Equals(other);

        public override int GetHashCode() => (int)Handle;

        public static bool operator ==(ShaderProgram? left, ShaderProgram? right) => Equals(left, right);
        public static bool operator !=(ShaderProgram? left, ShaderProgram? right) => !Equals(left, right);
    }
}
