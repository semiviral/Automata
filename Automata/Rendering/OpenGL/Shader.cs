#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Automata.Numerics;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class Shader : IDisposable
    {
        private static readonly Dictionary<UniformType, Type> _GLUniformTypeToRuntimeType = new Dictionary<UniformType, Type>
        {
            { UniformType.Bool, typeof(bool) },
            { UniformType.Double, typeof(double) },
            { UniformType.Float, typeof(float) },
            { UniformType.Int, typeof(int) },
            { UniformType.BoolVec2, typeof(Vector2b) },
            { UniformType.BoolVec3, typeof(Vector3b) },
            { UniformType.DoubleVec3, typeof(Vector3d) },
            { UniformType.FloatMat3x2, typeof(Matrix3x2) },
            { UniformType.FloatMat4, typeof(Matrix4x4) },
            { UniformType.IntVec2, typeof(Vector2i) },
            { UniformType.IntVec3, typeof(Vector3i) },
            { UniformType.Sampler2D, typeof(Texture2D) },
            { UniformType.UnsignedInt, typeof(uint) }
        };

        private const string _DEFAULT_VERTEX_SHADER =
            @"
                #version 330 core

                layout (location = 0) in vec3 vPos;

                uniform mat4 View; 
                uniform mat4 Projection; 

                out vec4 fColor;

                void main()
                {
                    gl_Position = Projection * View * vec4(vPos, 1.0);
                    fColor = vec4(1.0, 1.0, 1.0, 1.0);
                }
            ";

        private const string _DEFAULT_FRAGMENT_SHADER =
            @"
                #version 330 core

                in vec4 fColor;
                out vec4 fragColor;

                void main()
                {
                    fragColor = fColor;
                }
            ";

        private readonly GL _GL;
        private readonly uint _Handle;
        private readonly Dictionary<string, int> _CachedUniformLocations;

        public unsafe Shader()
        {
            _GL = GLAPI.Instance.GL;

            uint vertexShaderHandle = LoadShader(ShaderType.VertexShader, _DEFAULT_VERTEX_SHADER);
            uint fragmentShaderHandle = LoadShader(ShaderType.FragmentShader, _DEFAULT_FRAGMENT_SHADER);

            _Handle = _GL.CreateProgram();
            CreateShader(vertexShaderHandle, fragmentShaderHandle);

            int uniformCount;
            _GL.GetProgram(_Handle, GLEnum.ActiveUniforms, &uniformCount);
            _CachedUniformLocations = new Dictionary<string, int>();
            CacheKnownUniforms(uniformCount);
        }

        public unsafe Shader(string vertexPath, string fragmentPath)
        {
            _GL = GLAPI.Instance.GL;

            uint vertexShaderHandle = LoadShader(ShaderType.VertexShader, File.ReadAllText(vertexPath));
            uint fragmentShaderHandle = LoadShader(ShaderType.FragmentShader, File.ReadAllText(fragmentPath));

            _Handle = _GL.CreateProgram();
            CreateShader(vertexShaderHandle, fragmentShaderHandle);

            int uniformCount;
            _GL.GetProgram(_Handle, GLEnum.ActiveUniforms, &uniformCount);
            _CachedUniformLocations = new Dictionary<string, int>();
            CacheKnownUniforms(uniformCount);
        }

        private void CreateShader(uint vertexShaderHandle, uint fragmentShaderHandle)
        {
            _GL.AttachShader(_Handle, vertexShaderHandle);
            _GL.AttachShader(_Handle, fragmentShaderHandle);
            _GL.LinkProgram(_Handle);

            string infoLog = _GL.GetProgramInfoLog(_Handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new ShaderProgramException(infoLog);
            }

            _GL.DetachShader(_Handle, vertexShaderHandle);
            _GL.DetachShader(_Handle, fragmentShaderHandle);
            _GL.DeleteShader(vertexShaderHandle);
            _GL.DeleteShader(fragmentShaderHandle);
        }

        private void CacheKnownUniforms(int uniformCount)
        {
            for (uint uniformIndex = 0; uniformIndex < uniformCount; uniformIndex++)
            {
                string name = _GL.GetActiveUniform(_Handle, uniformIndex, out _, out _);
                int location = _GL.GetUniformLocation(_Handle, name);
                _CachedUniformLocations.Add(name, location);
            }
        }

        public void Dispose()
        {
            _GL.DeleteProgram(_Handle);
        }

        public void Use()
        {
            _GL.UseProgram(_Handle);
        }

        #region Set .. Get .. With Throw

        public void SetUniform(string name, int value)
        {
            int location = GetUniformLocation(name);

            _GL.ProgramUniform1(_Handle, location, value);
        }

        public void SetUniform(string name, float value)
        {
            int location = GetUniformLocation(name);

            _GL.ProgramUniform1(_Handle, location, value);
        }

        public void SetUniform(string name, Vector4 value)
        {
            int location = GetUniformLocation(name);

            _GL.ProgramUniform4(_Handle, location, value);
        }

        public void SetUniform(string name, Matrix4x4 value)
        {
            int location = GetUniformLocation(name);

            _GL.ProgramUniformMatrix4(_Handle, location, 1, false, AutomataMath.UnrollMatrix4x4(value).ToArray());
        }

        private int GetUniformLocation(string name)
        {
            if (_CachedUniformLocations.TryGetValue(name, out int location))
            {
                return location;
            }
            else
            {
                throw new UniformNotFoundException(name);
            }
        }

        #endregion

        #region Set .. Get .. As Try

        public bool TrySetUniform(string name, int value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform1(_Handle, location, value);

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
                _GL.ProgramUniform1(_Handle, location, value);

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
                _GL.ProgramUniform4(_Handle, location, value);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrySetUniform(string name, Matrix4x4 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniformMatrix4(_Handle, location, 1, false, AutomataMath.UnrollMatrix4x4(value).ToArray());

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryGetUniformLocation(string name, out int location) => _CachedUniformLocations.TryGetValue(name, out location);

        #endregion

        private uint LoadShader(ShaderType shaderType, string shader)
        {
            uint handle = _GL.CreateShader(shaderType);

            _GL.ShaderSource(handle, shader);
            _GL.CompileShader(handle);

            string infoLog = _GL.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new ShaderLoadException(shaderType, infoLog);
            }

            return handle;
        }
    }
}
