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
                typeof(RenderedShader),
                typeof(RenderedMeshComponent)
            };

            if (GLAPI.Instance == null)
            {
                throw new InvalidOperationException($"Singleton '{GLAPI.Instance}' has not been instantiated.");
            }

            _GL = GLAPI.Instance.GL;
        }

        public override unsafe void Update(EntityManager entityManager, float deltaTime)
        {
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach ((RenderedShader renderedShader, RenderedMeshComponent renderedMeshComponent) in entityManager
                .GetComponents<RenderedShader, RenderedMeshComponent>())
            {
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
