using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Shaders
{
    public class ProgramRegistry : Singleton<ProgramRegistry>, IDisposable
    {
        public const string RESERVED_UNIFORM_NAME_MATRIX_MVP = "_mvp";
        public const string RESERVED_UNIFORM_NAME_MATRIX_WORLD = "_world";
        public const string RESERVED_UNIFORM_NAME_MATRIX_OBJECT = "_object";
        public const string RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION = "_camera";
        public const string RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS = "_projectionParams";
        public const string RESERVED_UNIFORM_NAME_VEC4_VIEWPORT = "_viewport";

        private readonly Dictionary<string, ShaderProgram> _CachedVertexPrograms;
        private readonly Dictionary<string, ShaderProgram> _CachedFragmentPrograms;
        private readonly Dictionary<string, ProgramPipeline> _CachedProgramPipelines;

        public bool Disposed { get; private set; }

        public ProgramRegistry()
        {
            _CachedVertexPrograms = new Dictionary<string, ShaderProgram>();
            _CachedFragmentPrograms = new Dictionary<string, ShaderProgram>();
            _CachedProgramPipelines = new Dictionary<string, ProgramPipeline>();
        }

        public ProgramPipeline Load(string vertexShaderPath, string fragmentShaderPath)
        {
            const string compound_shader_key_format = "{0}:{1}";
            string compoundProgramKey = string.Format(compound_shader_key_format, vertexShaderPath, fragmentShaderPath);

            if (_CachedProgramPipelines.TryGetValue(compoundProgramKey, out ProgramPipeline? programPipeline))
            {
                return programPipeline;
            }
            else
            {
                if (!_CachedVertexPrograms.TryGetValue(vertexShaderPath, out ShaderProgram? vertexShader))
                {
                    vertexShader = new ShaderProgram(GLAPI.Instance.GL, ShaderType.VertexShader, vertexShaderPath);
                    _CachedVertexPrograms.Add(vertexShaderPath, vertexShader);
                }

                if (!_CachedFragmentPrograms.TryGetValue(fragmentShaderPath, out ShaderProgram? fragmentShader))
                {
                    fragmentShader = new ShaderProgram(GLAPI.Instance.GL, ShaderType.FragmentShader, fragmentShaderPath);
                    _CachedFragmentPrograms.Add(fragmentShaderPath, fragmentShader);
                }

                programPipeline = new ProgramPipeline(GLAPI.Instance.GL, vertexShader, fragmentShader);
                _CachedProgramPipelines.Add(compoundProgramKey, programPipeline);
                return programPipeline;
            }
        }


        #region IDisposable

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            DisposeInternal();
            Disposed = true;
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            foreach ((_, ShaderProgram shaderProgram) in _CachedVertexPrograms)
            {
                shaderProgram.Dispose();
            }

            foreach ((_, ShaderProgram shaderProgram) in _CachedFragmentPrograms)
            {
                shaderProgram.Dispose();
            }

            foreach ((_, ProgramPipeline programPipeline) in _CachedProgramPipelines)
            {
                programPipeline.Dispose();
            }
        }

        #endregion
    }
}
