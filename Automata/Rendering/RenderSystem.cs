#region

using System;
using Automata.Core;
using Automata.Core.Systems;
using Automata.Singletons;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderSystem : ComponentSystem
    {
        private readonly GL _GL;


        public RenderSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(Mesh)
            };

            GLAPI.Validate();
            _GL = GLAPI.Instance.GL;
        }

        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            _GL.ClearColor(delta, delta, delta, 1.0f);
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach (Camera camera in entityManager.GetComponents<Camera>())
            {
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
    }
}
