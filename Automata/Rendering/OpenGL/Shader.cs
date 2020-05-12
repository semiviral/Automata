#region

using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class Shader : IDisposable
    {
        private const string _DEFAULT_VERTEX_SHADER =
            @"#version 330 core
    
            layout (location = 0) in vec3 vPos;
            layout (location = 1) in vec4 vColor;
    
            uniform mat4 projection;
            uniform mat4 view;
            uniform mat4 model;
    
            out vec4 fColor;
    
            void main()
            {
                gl_Position = projection * view * model * vec4(vPos, 1.0);
                vec4 color = vColor;
                fColor = color;
            }";

        private const string _DEFAULT_FRAGMENT_SHADER =
            @"#version 330 core

            in vec4 fColor;
            out vec4 fragColor;

            void main()
            {
                fragColor = fColor;
            }";

        private readonly uint _Handle;
        private readonly GL _GL;

        public Shader()
        {
            _GL = GL.GetApi();

            uint vertexShader = LoadShader(ShaderType.VertexShader, _DEFAULT_VERTEX_SHADER);
            uint fragmentShader = LoadShader(ShaderType.FragmentShader, _DEFAULT_FRAGMENT_SHADER);

            _Handle = _GL.CreateProgram();
            _GL.AttachShader(_Handle, vertexShader);
            _GL.AttachShader(_Handle, fragmentShader);
            _GL.LinkProgram(_Handle);

            string infoLog = _GL.GetProgramInfoLog(_Handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new ShaderProgramException(infoLog);
            }

            _GL.DetachShader(_Handle, vertexShader);
            _GL.DetachShader(_Handle, fragmentShader);
            _GL.DeleteShader(vertexShader);
            _GL.DeleteShader(fragmentShader);
        }

        public Shader(string vertexPath, string fragmentPath)
        {
            _GL = GL.GetApi();

            uint vertexShader = LoadShader(ShaderType.VertexShader, File.ReadAllText(vertexPath));
            uint fragmentShader = LoadShader(ShaderType.FragmentShader, File.ReadAllText(fragmentPath));

            _Handle = _GL.CreateProgram();
            _GL.AttachShader(_Handle, vertexShader);
            _GL.AttachShader(_Handle, fragmentShader);
            _GL.LinkProgram(_Handle);

            string infoLog = _GL.GetProgramInfoLog(_Handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new ShaderProgramException(infoLog);
            }

            _GL.DetachShader(_Handle, vertexShader);
            _GL.DetachShader(_Handle, fragmentShader);
            _GL.DeleteShader(vertexShader);
            _GL.DeleteShader(fragmentShader);
        }

        public void Use()
        {
            _GL.UseProgram(_Handle);
        }

        public void SetUniform(string name, int value)
        {
            int location = GetUniformLocation(name);

            Use();
            _GL.Uniform1(location, value);
        }

        public void SetUniform(string name, float value)
        {
            int location = GetUniformLocation(name);

            Use();
            _GL.Uniform1(location, value);
        }

        public void SetUniform(string name, ref Vector4 vector4)
        {
            int location = GetUniformLocation(name);

            Use();
            _GL.Uniform4(location, ref vector4);
        }

        public void SetUniform(string name, Matrix4x4 value)
        {
            int location = GetUniformLocation(name);

            Use();
            _GL.UniformMatrix4(location, 1, false, Mathf.UnrollMatrix4x4(value));
        }

        private int GetUniformLocation(string name)
        {
            int location = _GL.GetUniformLocation(_Handle, name);

            if (location == -1)
            {
                throw new UniformNotFoundException(name);
            }

            return location;
        }

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

        public void Dispose()
        {
            _GL.DeleteProgram(_Handle);
        }
    }
}
