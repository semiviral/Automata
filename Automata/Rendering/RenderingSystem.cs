#region

using System;
using Automata.Core;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderingSystem : ComponentSystem
    {
        private readonly GL _GL;

        public RenderingSystem()
        {
            UtilizedComponentTypes = new[]
            {
                typeof(RenderedShaderComponent),
                typeof(RenderedMeshComponent)
            };

            _GL = GL.GetApi();
        }

        public override unsafe void Update(EntityManager entityManager, float deltaTime)
        {
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<RenderedShaderComponent, RenderedMeshComponent>())
            {
                RenderedShaderComponent renderedShaderComponent = entity.GetComponent<RenderedShaderComponent>();
                RenderedMeshComponent renderedMeshComponent = entity.GetComponent<RenderedMeshComponent>();

                if (renderedShaderComponent.Shader == null)
                {
                    throw new NullReferenceException(nameof(renderedShaderComponent.Shader));
                }
                else if (renderedMeshComponent.BufferObject == null)
                {
                    throw new NullReferenceException(nameof(renderedMeshComponent.BufferObject));
                }
                else if (renderedMeshComponent.VertexArrayObject == null)
                {
                    throw new NullReferenceException(nameof(renderedMeshComponent.VertexArrayObject));
                }

                renderedMeshComponent.VertexArrayObject.Bind();
                renderedShaderComponent.Shader.Use();

                _GL.DrawElements(PrimitiveType.Triangles, renderedMeshComponent.BufferObject.Length, DrawElementsType.UnsignedInt, null);
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

            foreach (RenderedShaderComponent renderedShaderComponent in entityManager.GetComponents<RenderedShaderComponent>())
            {
                renderedShaderComponent.Shader?.Dispose();
            }
        }
    }
}
