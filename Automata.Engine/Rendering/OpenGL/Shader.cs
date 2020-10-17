#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Serilog;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Engine.Rendering.OpenGL
{
    public class Shader : IDisposable
    {
        public const string RESERVED_UNIFORM_NAME_MATRIX_MV = "_mv";
        public const string RESERVED_UNIFORM_NAME_MATRIX_MVP = "_mvp";
        public const string RESERVED_UNIFORM_NAME_MATRIX_WORLD = "_world";
        public const string RESERVED_UNIFORM_NAME_MATRIX_OBJECT = "_object";
        public const string RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION = "_camera";
        public const string RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS = "_projectionParams";
        public const string RESERVED_UNIFORM_NAME_VEC4_VIEWPORT = "_viewport";

        private static readonly string[] _ReservedUniformNames =
        {
            RESERVED_UNIFORM_NAME_MATRIX_MV,
            RESERVED_UNIFORM_NAME_MATRIX_MVP,
            RESERVED_UNIFORM_NAME_MATRIX_WORLD,
            RESERVED_UNIFORM_NAME_MATRIX_OBJECT,
            RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION,
            RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS,
            RESERVED_UNIFORM_NAME_VEC4_VIEWPORT,
        };

        private static readonly string _DefaultVertexShader =
            $@"
                #version 330 core

                layout (location = 0) in vec3 vertex;

                out vec4 fragColor;

                uniform mat4 {RESERVED_UNIFORM_NAME_MATRIX_MVP};

                void main()
                {{
                    gl_Position = {RESERVED_UNIFORM_NAME_MATRIX_MVP} * vec4(vertex, 1.0);
                    fragColor = vec4(1.0);
                }}
            ";

        private static readonly string _DefaultFragmentShader =
            @"
                #version 330 core

                in vec4 fragColor;
                out vec4 outColor;

                void main()
                {
                    outColor = fragColor;
                }
            ";

        private readonly GL _GL;
        private readonly uint _Handle;
        private readonly Dictionary<string, int> _CachedUniformLocations;

        public bool HasAutomataUniforms { get; }

        public unsafe Shader()
        {
            _GL = GLAPI.Instance.GL;

            uint vertexShaderHandle = CreateGPUShader(ShaderType.VertexShader, _DefaultVertexShader);
            uint fragmentShaderHandle = CreateGPUShader(ShaderType.FragmentShader, _DefaultFragmentShader);

            _Handle = _GL.CreateProgram();
            CreateShader(vertexShaderHandle, fragmentShaderHandle);

            int uniformCount;
            _GL.GetProgram(_Handle, GLEnum.ActiveUniforms, &uniformCount);
            _CachedUniformLocations = new Dictionary<string, int>();
            CacheUniforms(uniformCount);
            HasAutomataUniforms = _CachedUniformLocations.Keys.Intersect(_ReservedUniformNames).Any();
        }

        public unsafe Shader(uint vertexShaderHandle, uint fragmentShaderHandle)
        {
            _GL = GLAPI.Instance.GL;

            _Handle = _GL.CreateProgram();

            try
            {
                CreateShader(vertexShaderHandle, fragmentShaderHandle);
            }
            catch (ShaderLoadException ex)
            {
                Log.Error($"Failed to load {ex.Type} shader (will use fallback to default): {ex.InfoLog}");

                vertexShaderHandle = CreateGPUShader(ShaderType.VertexShader, _DefaultVertexShader);
                fragmentShaderHandle = CreateGPUShader(ShaderType.FragmentShader, _DefaultFragmentShader);
                CreateShader(vertexShaderHandle, fragmentShaderHandle);
            }
            finally
            {
                int uniformCount;
                _GL.GetProgram(_Handle, GLEnum.ActiveUniforms, &uniformCount);
                _CachedUniformLocations = new Dictionary<string, int>();
                CacheUniforms(uniformCount);
                HasAutomataUniforms = _CachedUniformLocations.Keys.Intersect(_ReservedUniformNames).Any();
            }
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

        private void CacheUniforms(int uniformCount)
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

        public bool TrySetUniform(string name, Vector3 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform3(_Handle, location, value);

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

        public unsafe bool TrySetUniform(string name, Matrix4x4 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                Span<float> span = stackalloc float[sizeof(Matrix4x4) / sizeof(float)];
                value.UnrollInto(ref span);

                _GL.ProgramUniformMatrix4(_Handle, location, 1, false, span);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryGetUniformLocation(string name, out int location) => _CachedUniformLocations.TryGetValue(name, out location);

        #endregion

        public static Shader LoadShader(string vertexShaderPath, string fragmentShaderPath)
        {
            uint vertexShaderHandle = CreateGPUShader(ShaderType.VertexShader, File.ReadAllText(vertexShaderPath));
            uint fragmentShaderHandle = CreateGPUShader(ShaderType.FragmentShader, File.ReadAllText(fragmentShaderPath));
            Shader shader = new Shader(vertexShaderHandle, fragmentShaderHandle);

            Log.Debug($"({nameof(Shader)}) Loaded vertex shader from: {vertexShaderPath}");
            Log.Debug($"({nameof(Shader)}) Loaded fragment shader from: {fragmentShaderPath}");

            return shader;
        }

        private static uint CreateGPUShader(ShaderType shaderType, string shader)
        {
            GL gl = GLAPI.Instance.GL;

            uint handle = gl.CreateShader(shaderType);

            gl.ShaderSource(handle, shader);
            gl.CompileShader(handle);

            string infoLog = gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new ShaderLoadException(shaderType, infoLog);
            }

            return handle;
        }
    }
}
