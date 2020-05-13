#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Automata.Core;
using Automata.Core.Systems;
using Automata.Singletons;
using Silk.NET.OpenGL;
using Vortice.Mathematics;

#endregion

namespace Automata.Rendering
{
    public class RenderSystem : ComponentSystem
    {
        private readonly GL _GL;

        private bool _HasGameWindowResized;
        private Vector2 _ResizedSize;

        public RenderSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(Mesh)
            };

            GLAPI.Validate();
            _GL = GLAPI.Instance.GL;

            GameWindow.Validate();

            Debug.Assert(GameWindow.Instance != null);
            Debug.Assert(GameWindow.Instance.Window != null);

            GameWindowResized(GameWindow.Instance.Window.Size);
            GameWindow.Instance.Window.Resize += GameWindowResized;
        }

        public override unsafe void Update(EntityManager entityManager, float deltaTime)
        {
            _GL.ClearColor(deltaTime, deltaTime, deltaTime, 1.0f);
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach (Camera camera in entityManager.GetComponents<Camera>())
            {
                if (_HasGameWindowResized)
                {
                    camera.Projection = Matrix4x4.CreatePerspective(AutomataMath.ToRadians(90f), _ResizedSize.X / _ResizedSize.Y, 0.1f, 100f);
                }

                if (camera.Shader == null)
                {
                    continue;
                }

                camera.Shader.Use();

                foreach (Mesh mesh in entityManager.GetComponents<Mesh>())
                {
                    if (mesh.IndexesBuffer == null)
                    {
                        continue;
                    }
                    else if (mesh.VertexArrayObject == null)
                    {
                        continue;
                    }

                    mesh.VertexArrayObject.Bind();

                    _GL.DrawElements(PrimitiveType.Triangles, mesh.IndexesBuffer.Length, DrawElementsType.UnsignedInt, null);

                    if (_GL.GetError() != GLEnum.NoError)
                    {
                        throw new Exception();
                    }
                }
            }
        }

        public override void Destroy(EntityManager entityManager)
        {
            foreach (Mesh renderedMeshComponent in entityManager.GetComponents<Mesh>())
            {
                renderedMeshComponent.VertexesBuffer?.Dispose();
                renderedMeshComponent.IndexesBuffer?.Dispose();
                renderedMeshComponent.VertexArrayObject?.Dispose();
            }
        }

        private void GameWindowResized(Size size)
        {
            _HasGameWindowResized = true;
            _ResizedSize = new Vector2(size.Width, size.Height);
        }
    }
}
