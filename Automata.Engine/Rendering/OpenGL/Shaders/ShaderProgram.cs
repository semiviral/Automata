using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Automata.Engine.Extensions;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Shaders
{
    public class ShaderProgram : OpenGLObject, IEquatable<ShaderProgram>
    {
        private readonly Dictionary<string, int> _CachedUniforms;

        public ShaderType Type { get; }

        public unsafe ShaderProgram(GL gl, ShaderType shaderType, string path) : base(gl)
        {
            void CacheUniformsImpl()
            {
                GL.GetProgram(Handle, ProgramPropertyARB.ActiveUniforms, out int uniformCount);

                for (uint uniformIndex = 0; uniformIndex < uniformCount; uniformIndex++)
                {
                    string name = GL.GetActiveUniform(Handle, uniformIndex, out _, out _);
                    int location = GL.GetUniformLocation(Handle, name);
                    _CachedUniforms.Add(name, location);
                }
            }

            _CachedUniforms = new Dictionary<string, int>();
            Type = shaderType;

            // this is kinda dumb, right?
            byte* shader = (byte*)SilkMarshal.StringToPtr(File.ReadAllText(path));
            Handle = GL.CreateShaderProgram(Type, 1, shader);
            CheckInfoLogAndThrow();
            CacheUniformsImpl();

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ShaderProgram), $"Loaded ({Type}): {path}"));
        }

        private void CheckInfoLogAndThrow()
        {
            GetInfoLog(out string infoLog);

            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new ShaderLoadException(Type, infoLog);
            }
        }

        [SkipLocalsInit]
        public unsafe void GetInfoLog(out string infoLog)
        {
            GL.GetProgram(Handle, ProgramPropertyARB.InfoLogLength, out int infoLogLength);

            if (infoLogLength is 0)
            {
                infoLog = string.Empty;
                return;
            }

            Span<byte> infoLogSpan = stackalloc byte[infoLogLength];
            GL.GetProgramInfoLog(Handle, (uint)infoLogLength, (uint*)&infoLogLength, infoLogSpan);
            infoLog = Encoding.ASCII.GetString(infoLogSpan);
        }


        #region Uniforms

        public bool TrySetUniform(string name, int value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                GL.ProgramUniform1(Handle, location, value);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrySetUniform(string name, float value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                GL.ProgramUniform1(Handle, location, value);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrySetUniform(string name, Vector3 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                GL.ProgramUniform3(Handle, location, value);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrySetUniform(string name, Vector4 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                GL.ProgramUniform4(Handle, location, value);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrySetUniform(string name, Matrix4x4 value, bool transpose = false)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                GL.ProgramUniformMatrix4(Handle, location, 1, transpose, value.Unroll<Matrix4x4, float>());
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryGetUniformLocation(string name, out int location) => _CachedUniforms.TryGetValue(name, out location);

        #endregion


        #region IDisposable

        protected override void CleanupNativeResources() => GL.DeleteProgram(Handle);

        #endregion


        #region IEquateable

        public bool Equals(ShaderProgram? other) => other is not null && (other.Handle == Handle);
        public override bool Equals(object? obj) => obj is ShaderProgram other && Equals(other);

        public override int GetHashCode() => (int)Handle;

        public static bool operator ==(ShaderProgram? left, ShaderProgram? right) => Equals(left, right);
        public static bool operator !=(ShaderProgram? left, ShaderProgram? right) => !Equals(left, right);

        #endregion
    }
}
