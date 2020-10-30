#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public const string RESERVED_UNIFORM_NAME_MATRIX_MVP = "_mvp";
        public const string RESERVED_UNIFORM_NAME_MATRIX_WORLD = "_world";
        public const string RESERVED_UNIFORM_NAME_MATRIX_OBJECT = "_object";
        public const string RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION = "_camera";
        public const string RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS = "_projectionParams";
        public const string RESERVED_UNIFORM_NAME_VEC4_VIEWPORT = "_viewport";

        private static readonly string[] _ReservedUniformNames =
        {
            RESERVED_UNIFORM_NAME_MATRIX_MVP,
            RESERVED_UNIFORM_NAME_MATRIX_WORLD,
            RESERVED_UNIFORM_NAME_MATRIX_OBJECT,
            RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION,
            RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS,
            RESERVED_UNIFORM_NAME_VEC4_VIEWPORT
        };

        private static readonly string _DefaultVertexShaderPacked =
            $@"
                #version 330 core

                layout (location = 0) in vec3 vertex;

                out vec4 fragColor;

                uniform mat4 {RESERVED_UNIFORM_NAME_MATRIX_MVP};

                void main()
                {{
                    gl_Position = {RESERVED_UNIFORM_NAME_MATRIX_MVP} * vec4(vertex, 1.0);
                    fragColor = vec4(normalize(vertex), 1.0);
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

        private static readonly Dictionary<string, Shader> _CachedShaders = new Dictionary<string, Shader>();
        private readonly Dictionary<string, int> _CachedUniformLocations;

        private readonly GL _GL;
        private readonly uint _Handle;

        public Guid ID { get; }
        public bool HasAutomataUniforms { get; }

        internal Shader()
        {
            _GL = GLAPI.Instance.GL;
            ID = Guid.NewGuid();

            uint vertexShaderHandle = CreateGPUShader(ShaderType.VertexShader, _DefaultVertexShaderPacked);
            uint fragmentShaderHandle = CreateGPUShader(ShaderType.FragmentShader, _DefaultFragmentShader);

            _Handle = _GL.CreateProgram();
            CreateShaderProgram(vertexShaderHandle, fragmentShaderHandle);

            _CachedUniformLocations = new Dictionary<string, int>();
            CacheShaderProgramUniformHandles();
            HasAutomataUniforms = _CachedUniformLocations.Keys.Intersect(_ReservedUniformNames).Any();
        }

        private Shader(uint vertexShaderHandle, uint fragmentShaderHandle)
        {
            _GL = GLAPI.Instance.GL;

            _Handle = _GL.CreateProgram();
            CreateShaderProgram(vertexShaderHandle, fragmentShaderHandle);

            _CachedUniformLocations = new Dictionary<string, int>();
            CacheShaderProgramUniformHandles();
            HasAutomataUniforms = _CachedUniformLocations.Keys.Intersect(_ReservedUniformNames).Any();
        }

        private void CreateShaderProgram(uint vertexShaderHandle, uint fragmentShaderHandle)
        {
            _GL.AttachShader(_Handle, vertexShaderHandle);
            _GL.AttachShader(_Handle, fragmentShaderHandle);
            _GL.LinkProgram(_Handle);

            string infoLog = _GL.GetProgramInfoLog(_Handle);
            if (!string.IsNullOrWhiteSpace(infoLog)) throw new ShaderProgramException(infoLog);

            _GL.DetachShader(_Handle, vertexShaderHandle);
            _GL.DetachShader(_Handle, fragmentShaderHandle);
            _GL.DeleteShader(vertexShaderHandle);
            _GL.DeleteShader(fragmentShaderHandle);
        }

        private void CacheShaderProgramUniformHandles()
        {
            _GL.GetProgram(_Handle, GLEnum.ActiveUniforms, out int uniformCount);

            for (uint uniformIndex = 0; uniformIndex < uniformCount; uniformIndex++)
            {
                string name = _GL.GetActiveUniform(_Handle, uniformIndex, out _, out _);
                int location = _GL.GetUniformLocation(_Handle, name);
                _CachedUniformLocations.Add(name, location);
            }
        }

        public void Use() => _GL.UseProgram(_Handle);

        public static bool TryLoadShaderWithCache(string vertexShaderPath, string fragmentShaderPath, [NotNullWhen(true)] out Shader? shader)
        {
            const string compound_shader_key_format = "{0}:{1}";
            string compoundShaderKey = string.Format(compound_shader_key_format, vertexShaderPath, fragmentShaderPath);

            if (_CachedShaders.TryGetValue(compoundShaderKey, out shader)) return true;
            else if (TryLoadShader(vertexShaderPath, fragmentShaderPath, out shader))
            {
                _CachedShaders.Add(compoundShaderKey, shader);
                return true;
            }
            else return false;
        }

        public static bool TryLoadShader(string vertexShaderPath, string fragmentShaderPath, [NotNullWhen(true)] out Shader? shader)
        {
            try
            {
                uint vertexShaderHandle = CreateGPUShader(ShaderType.VertexShader, File.ReadAllText(vertexShaderPath));
                uint fragmentShaderHandle = CreateGPUShader(ShaderType.FragmentShader, File.ReadAllText(fragmentShaderPath));
                shader = new Shader(vertexShaderHandle, fragmentShaderHandle);
            }
            catch (Exception exception)
            {
                Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Shader),
                    $"Failed to load shader: {exception.Message}\r\n{exception.StackTrace}"));

                shader = null;
                return false;
            }

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Shader), $"Loaded vertex shader from: {vertexShaderPath}"));
            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Shader), $"Loaded fragment shader from: {fragmentShaderPath}"));

            return true;
        }

        private static uint CreateGPUShader(ShaderType shaderType, string shader)
        {
            GL gl = GLAPI.Instance.GL;

            uint handle = gl.CreateShader(shaderType);

            gl.ShaderSource(handle, shader);
            gl.CompileShader(handle);

            string infoLog = gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog)) throw new ShaderLoadException(shaderType, infoLog);

            return handle;
        }

        public void Dispose() => _GL.DeleteProgram(_Handle);


        #region Set .. Get .. As Try

        public bool TrySetUniform(string name, int value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform1(_Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, float value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform1(_Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, Vector3 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform3(_Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, Vector4 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniform4(_Handle, location, value);

                return true;
            }
            else return false;
        }

        public bool TrySetUniform(string name, Matrix4x4 value)
        {
            if (TryGetUniformLocation(name, out int location))
            {
                _GL.ProgramUniformMatrix4(_Handle, location, 1, false, value.Unroll());
                return true;
            }
            else return false;
        }

        private bool TryGetUniformLocation(string name, out int location) => _CachedUniformLocations.TryGetValue(name, out location);

        #endregion
    }
}
