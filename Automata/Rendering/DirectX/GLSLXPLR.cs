#region

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Serilog;

#endregion

namespace Automata.Rendering.DirectX
{
    public class GLSLXPLR : Singleton<GLSLXPLR>
    {
        private const string _GLSLC_ARGUMENTS_FORMAT = "-o \"{0}\" \"{1}\"";

        private const string _TEST_SHADER =
            @"
                #version 450
                #pragma shader_stage(fragment)
    
                layout(location = 0) in vec3 fragColor;
                
                layout(location = 0) out vec4 outColor;
                
                void main() {
                    outColor = vec4(fragColor, 1.0);
                }
            ";

        private static readonly string[] _AssociatedFiles =
        {
            "glslc.exe"
        };

        private Process? _TranspilerProcess;

        public GLSLXPLR()
        {
            AssignSingletonInstance(this);

            ValidateTranspiler();
        }

        private void ValidateTranspiler()
        {
            Log.Information(string.Format(_LogFormat, "Validating transpiler."));

            foreach (string fileName in _AssociatedFiles)
            {
                if (!File.Exists($"{Environment.CurrentDirectory}/{fileName}"))
                {
                    Log.Error(string.Format(_LogFormat, $"Transpiler missing required file: {fileName}"));
                    return;
                }
            }

            _TranspilerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $"{Environment.CurrentDirectory}/{_AssociatedFiles[0]}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = false
                }
            };

            Log.Information(string.Format(_LogFormat, "Testing shader transpilation."));

            string temporaryShader = Path.GetTempFileName();
            File.WriteAllText(temporaryShader, _TEST_SHADER);

            if (TryTranspileShader(temporaryShader, out _))
            {
                Log.Information(string.Format(_LogFormat, "Successfully validated."));
            }
            else
            {
                Log.Error(string.Format(_LogFormat, "Failed to transpile test shader."));
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

            Log.Information(string.Format(_LogFormat, $"Transpiling shader: {shaderPath}"));

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

            if (error)
            {
                return false;
            }

            shaderBytes = File.ReadAllBytes(tempOutputFile);

            return true;
        }
    }
}
