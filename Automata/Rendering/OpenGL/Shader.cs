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
        public static string DefaultVertexShader =
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

        public static string DefaultFragmentShader =
            @"#version 330 core


            in vec4 fColor;
            out vec4 FragColor;

            void main()
            {
                FragColor = fColor;
            }";

        private readonly uint _Handle;
        private readonly GL _GL;

        public Shader(GL gl)
        {
            _GL = gl;

            uint vertexShader = LoadShader(ShaderType.VertexShader, DefaultVertexShader);
            uint fragmentShader = LoadShader(ShaderType.FragmentShader, DefaultFragmentShader);

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

        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            _GL = gl;

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
            int location = _GL.GetUniformLocation(_Handle, name);

            if (location == -1)
            {
                throw new UniformNotFoundException(name);
            }

            Use();
            _GL.Uniform1(location, value);
        }

        public void SetUniform(string name, float value)
        {
            int location = _GL.GetUniformLocation(_Handle, name);

            if (location == -1)
            {
                throw new UniformNotFoundException(name);
            }

            Use();
            _GL.Uniform1(location, value);
        }

        public void SetUniform(string name, Matrix4x4 value)
        {
            int location = _GL.GetUniformLocation(_Handle, name);

            if (location == -1)
            {
                throw new UniformNotFoundException(name);
            }

            Use();
            _GL.UniformMatrix4(location, 1, false, Mathf.UnrollMatrix4x4(value));
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
