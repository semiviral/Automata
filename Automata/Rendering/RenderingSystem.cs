#region

using System;
using Automata.Core;
using Automata.Core.Systems;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderingSystem : ComponentSystem
    {
        private readonly GL _GL;

        public RenderingSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(RenderedShader),
                typeof(RenderedMeshComponent)
            };

            _GL = GL.GetApi();
        }

        public override unsafe void Update(EntityManager entityManager, float deltaTime)
        {
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<RenderedShader, RenderedMeshComponent>())
            {
                RenderedShader renderedShader = entity.GetComponent<RenderedShader>();
                RenderedMeshComponent renderedMeshComponent = entity.GetComponent<RenderedMeshComponent>();

                if (renderedShader.Shader == null)
                {
                    throw new NullReferenceException(nameof(renderedShader.Shader));
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
                renderedShader.Shader.Use();

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

            foreach (RenderedShader renderedShaderComponent in entityManager.GetComponents<RenderedShader>())
            {
                renderedShaderComponent.Shader?.Dispose();
            }
        }
    }
}
