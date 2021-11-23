using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Shaders
{
    public class ProgramRegistry : Singleton<ProgramRegistry>, IDisposable
    {
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
            string compound_program_key = string.Format(compound_shader_key_format, vertexShaderPath, fragmentShaderPath);

            if (_CachedProgramPipelines.TryGetValue(compound_program_key, out ProgramPipeline? program_pipeline))
            {
                return program_pipeline!;
            }
            else
            {
                if (!_CachedVertexPrograms.TryGetValue(vertexShaderPath, out ShaderProgram? vertex_shader))
                {
                    vertex_shader = new ShaderProgram(GLAPI.Instance.GL, ShaderType.VertexShader, vertexShaderPath);
                    _CachedVertexPrograms.Add(vertexShaderPath, vertex_shader);
                }

                if (!_CachedFragmentPrograms.TryGetValue(fragmentShaderPath, out ShaderProgram? fragment_shader))
                {
                    fragment_shader = new ShaderProgram(GLAPI.Instance.GL, ShaderType.FragmentShader, fragmentShaderPath);
                    _CachedFragmentPrograms.Add(fragmentShaderPath, fragment_shader);
                }

                program_pipeline = new ProgramPipeline(GLAPI.Instance.GL, vertex_shader!, fragment_shader!);
                _CachedProgramPipelines.Add(compound_program_key, program_pipeline);
                return program_pipeline;
            }
        }


        #region IDisposable

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            foreach ((_, ShaderProgram shader_program) in _CachedVertexPrograms)
            {
                shader_program.Dispose();
            }

            foreach ((_, ShaderProgram shader_program) in _CachedFragmentPrograms)
            {
                shader_program.Dispose();
            }

            foreach ((_, ProgramPipeline program_pipeline) in _CachedProgramPipelines)
            {
                program_pipeline.Dispose();
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
