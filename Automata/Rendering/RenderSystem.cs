#region

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
                typeof(RenderedMeshComponent)
            };

            GLAPI.Validate();

            _GL = GLAPI.Instance.GL;
        }

        public override unsafe void Update(EntityManager entityManager, float deltaTime)
        {
            _GL.ClearColor(0.2f, 0.2f, 0.5f, 1.0f);
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach (Camera camera in entityManager.GetComponents<Camera>())
            {
                if (camera.Shader == null)
                {
                    continue;
                }

                foreach (RenderedMeshComponent renderedMeshComponent in entityManager.GetComponents<RenderedMeshComponent>())
                {
                    if (renderedMeshComponent.BufferObject == null)
                    {
                        continue;
                    }
                    else if (renderedMeshComponent.VertexArrayObject == null)
                    {
                        continue;
                    }

                    renderedMeshComponent.VertexArrayObject.Bind();
                    camera.Shader.Use();

                    _GL.DrawElements(PrimitiveType.Triangles, renderedMeshComponent.BufferObject.Length, DrawElementsType.UnsignedInt, null);
                }
            }
        }

        public override void Destroy(EntityManager entityManager)
        {
            foreach (RenderedMeshComponent renderedMeshComponent in entityManager.GetComponents<RenderedMeshComponent>())
            {
                renderedMeshComponent.VertexBuffer?.Dispose();
                renderedMeshComponent.BufferObject?.Dispose();
                renderedMeshComponent.VertexArrayObject?.Dispose();
            }
        }
    }
}
