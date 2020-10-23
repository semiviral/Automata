#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Serilog;

#endregion


namespace Automata.Engine.Rendering.DirectX
{
    public class GLSLXPLR : Singleton<GLSLXPLR>
    {
        private const string _GLSL_XPLR_EXECUTABLE = "glslc.exe";
        private const string _GLSLC_ARGUMENTS_FORMAT = "-o \"{0}\" \"{1}\"";

        private const string _DEFAULT_VERTEX =
            @"
                #version 450
                #pragma shader_stage(vertex)
                #extension GL_ARB_separate_shader_objects : enable

                vec2 positions[3] = vec2[]
                    (
                        vec2(0.0, -0.5),
                        vec2(0.5, 0.5),
                        vec2(-0.5, 0.5)
                    );

                vec3 colors[3] = vec3[]
                    (
                        vec3(1.0, 0.0, 0.0),
                        vec3(0.0, 1.0, 0.0),
                        vec3(0.0, 0.0, 1.0)
                    );

                layout (location = 0) out vec3 fragColor;

                void main()
                {
                    gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0);
                    fragColor = colors[gl_VertexIndex];
                }
            ";

        private const string _DEFAULT_FRAGMENT =
            @"
                #version 450
                #pragma shader_stage(fragment)
                #extension GL_ARB_separate_shader_objects : enable
    
                layout (location = 0) in vec3 fragColor;
                
                layout (location = 0) out vec4 outColor;
                
                void main() {
                    outColor = vec4(fragColor, 1.0);
                }
            ";

        private byte[]? _DefaultFragmentShader;
        private byte[]? _DefaultVertexShader;

        private Process? _TranspilerProcess;

        public IReadOnlyCollection<byte> DefaultVertexShader
        {
            get => _DefaultVertexShader ?? Array.Empty<byte>();
            set => _DefaultVertexShader = (byte[])value ?? throw new ArgumentNullException(nameof(value));
        }

        public IReadOnlyCollection<byte> DefaultFragmentShader
        {
            get => _DefaultFragmentShader ?? Array.Empty<byte>();
            set => _DefaultFragmentShader = (byte[])value ?? throw new ArgumentNullException(nameof(value));
        }

        public GLSLXPLR()
        {
            AssignSingletonInstance(this);

            ValidateTranspiler();
            TranspileDefaultShaders();
        }

        private void ValidateTranspiler()
        {
            Log.Information(string.Format(_LogFormat, "Validating transpiler."));

            if (!File.Exists($"{Environment.CurrentDirectory}/{_GLSL_XPLR_EXECUTABLE}"))
            {
                Log.Error(string.Format(_LogFormat, $"Transpiler missing required file: {_GLSL_XPLR_EXECUTABLE}"));
                return;
            }

            _TranspilerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $"{Environment.CurrentDirectory}/{_GLSL_XPLR_EXECUTABLE}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = false
                }
            };

            Log.Information(string.Format(_LogFormat, "Successfully validated."));
        }

        private void TranspileDefaultShaders()
        {
            Log.Information(string.Format(_LogFormat, "Transpiling default GLSL shaders."));

            string temporaryShader = Path.GetTempFileName();
            File.WriteAllText(temporaryShader, _DEFAULT_VERTEX);

            if (TryTranspileShader(temporaryShader, out _DefaultVertexShader)) Log.Information(string.Format(_LogFormat, "Transpiled default vertex shader."));
            else
            {
                Log.Error(string.Format(_LogFormat, "Failed to transpile vertex shader."));
                return;
            }

            File.WriteAllText(temporaryShader, _DEFAULT_FRAGMENT);

            if (TryTranspileShader(temporaryShader, out _DefaultFragmentShader))
                Log.Information(string.Format(_LogFormat, "Transpiled default fragment shader."));
            else
            {
                Log.Error(string.Format(_LogFormat, "Failed to transpile fragment shader."));
                return;
            }

            File.Delete(temporaryShader);
        }

        public bool TryTranspileShader(string shaderPath, [MaybeNullWhen(false)] out byte[] shaderBytes)
        {
            shaderBytes = null;

            if (_TranspilerProcess == null)
            {
                Log.Error(_LogFormat, "Transpiler not found.");
                return false;
            }
            else if (!File.Exists(shaderPath))
            {
                Log.Error(string.Format(_LogFormat, $"No file exists at: {shaderPath}"));
                return false;
            }

            string tempOutputFile = Path.GetTempFileName();

            _TranspilerProcess.StartInfo.Arguments = string.Format(_GLSLC_ARGUMENTS_FORMAT, tempOutputFile, shaderPath);
            _TranspilerProcess.Start();
            _TranspilerProcess.StartInfo.Arguments = string.Empty;

            bool error = false;

            while (!_TranspilerProcess.StandardError.EndOfStream)
            {
                Log.Error(string.Format(_LogFormat, _TranspilerProcess.StandardError.ReadLine()));
                error = true;
            }

            if (error) return false;

            shaderBytes = File.ReadAllBytes(tempOutputFile);
            File.Delete(tempOutputFile);

            return true;
        }
    }
}
