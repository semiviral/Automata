#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Automata;
using Automata.Core;
using Automata.Input;
using Automata.Rendering;
using Automata.Rendering.OpenGL;
using Serilog;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;

#endregion

namespace AutomataTest
{
    internal class Program
    {
        private static readonly string _localDataPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create);

        private static IWindow _Window;
        private static InputSystem _InputSystem;
        private static GL _GL;

        private static VertexBuffer _VBO;
        private static BufferObject<uint> _EBO;
        private static VertexArrayObject<float, uint> _VAO;
        private static Shader _Shader;

        //Vertex data, uploaded to the VBO.
        private static readonly Vector3[] _vertices =
        {
            //X    Y      Z     R  G  B  A
            new Vector3(0.5f, 0.5f, 0.0f),
            new Vector3(0.5f, -0.5f, 0.0f),
            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3(-0.5f, 0.5f, 0.0f)
        };

        private static readonly Color64[] _colors =
        {
            new Color64(1f, 0f, 0f, 1f),
            new Color64(0f, 0f, 0f, 1f),
            new Color64(0f, 0f, 1f, 1f),
            new Color64(0f, 0f, 0f, 1f)
        };

        private static readonly uint[] _indices =
        {
            0,
            1,
            3,
            1,
            2,
            3
        };

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Information("Static logger initialized.");

            if (!Directory.Exists($@"{_localDataPath}/Wyd/"))
            {
                Log.Information("Local data folder missing, creating...");
                Directory.CreateDirectory($@"{_localDataPath}/Wyd/");
            }


            WindowOptions options = WindowOptions.Default;
            options.Title = "Wyd: A Journey";
            options.Size = new Size(800, 600);
            options.Position = new Point(500, 400);

            _Window = Window.Create(options);
            _Window.Load += OnLoad;
            _Window.Render += OnRender;
            _Window.Update += OnUpdate;
            _Window.Closing += OnClose;
            _Projection = Matrix4x4.CreatePerspective(Mathf.ToRadians(90f),
                (float)_Window.Size.Width / _Window.Size.Height, 0.1f, 100f);

            _Window.Run();
        }

        private static void OnLoad()
        {
            _InputSystem = new InputSystem(_Window);

            Entity entity = new Entity();
            EntityManager.RegisterEntity(entity);
            EntityManager.RegisterComponent<KeyboardInputComponent>(entity);

            _GL = GL.GetApi();

            _VBO = new VertexBuffer(_GL);
            _VBO.SetBufferData(_vertices, _colors);
            _EBO = new BufferObject<uint>(_GL, BufferTargetARB.ElementArrayBuffer);
            _EBO.SetBufferData(_indices);
            _VAO = new VertexArrayObject<float, uint>(_GL, _VBO, _EBO);

            _VAO.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
            _VAO.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);

            _Shader = new Shader(_GL, "default.vert", "shader.frag");
            _Shader.SetUniform("model", Matrix4x4.Identity);
            _Shader.SetUniform("projection", _Projection);
            _Shader.SetUniform("view", _View);
        }

        private static Matrix4x4 _View;
        private static Matrix4x4 _Projection;
        private static readonly Glfw _glfw = Glfw.GetApi();

        private static unsafe void OnRender(double delta)
        {
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            _VAO.Bind();
            _Shader.Use();

            const float radius = 10f;
            //_View = Matrix4x4.CreateLookAt(new Vector3((float)Math.Sin(radius * _glfw.GetTime()), 0f, (float)Math.Cos(radius * _glfw.GetTime())), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f));

            _Shader.SetUniform("uBlue", (float)Math.Sin((DateTime.UtcNow.Millisecond / 1000f) * Math.PI));


            _GL.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, null);
        }

        private static void OnUpdate(double delta)
        {
            SystemManager.GlobalUpdate();
        }

        private static void OnClose()
        {
            _VBO.Dispose();
            _EBO.Dispose();
            _VAO.Dispose();
            _Shader.Dispose();
        }
    }
}
