#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Automata.Core;
using Automata.Rendering.OpenGL;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    /// <summary>
    ///     Consumes a <see cref="PendingMeshDataComponent" /> and creates relevant GPU buffers so a given mesh can be
    ///     rendered.
    /// </summary>
    public class MeshCompositionSystem : ComponentSystem
    {
        /// <summary>
        ///     <see cref="GL" /> instance to use for graphics operations.
        /// </summary>
        private readonly GL _GL;

        /// <summary>
        ///     Default shader to apply to graphics entities.
        /// </summary>
        private readonly Shader _DefaultShader;

        public MeshCompositionSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(PendingMeshDataComponent)
            };

            _GL = GL.GetApi();
            _DefaultShader = new Shader();
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            List<IEntity> entities = entityManager.GetEntitiesWithComponents<PendingMeshDataComponent>().ToList();

            foreach (IEntity entity in entities)
            {
                // create a shader component if one doesn't exist on object
                if (!entity.TryGetComponent(out RenderedShaderComponent _))
                {
                    entityManager.RegisterComponent(entity, new RenderedShaderComponent
                    {
                        Shader = _DefaultShader
                    });
                }

                // create gpu buffers object if one doesn't exist on entity
                if (!entity.TryGetComponent(out RenderedMeshComponent renderedMeshComponent))
                {
                    renderedMeshComponent = new RenderedMeshComponent
                    {
                        VertexBuffer = new VertexBuffer(_GL),
                        BufferObject = new BufferObject<uint>(_GL, BufferTargetARB.ElementArrayBuffer),
                    };
                    renderedMeshComponent.VertexArrayObject =
                        new VertexArrayObject<float, uint>(_GL, renderedMeshComponent.VertexBuffer, renderedMeshComponent.BufferObject);

                    entityManager.RegisterComponent(entity, renderedMeshComponent);
                }

                // null checks for C#8 null safety
                if (renderedMeshComponent.VertexBuffer == null)
                {
                    throw new NullReferenceException(nameof(renderedMeshComponent.VertexBuffer));
                }
                else if (renderedMeshComponent.BufferObject == null)
                {
                    throw new NullReferenceException(nameof(renderedMeshComponent.BufferObject));
                }
                else if (renderedMeshComponent.VertexArrayObject == null)
                {
                    throw new NullReferenceException(nameof(renderedMeshComponent.VertexArrayObject));
                }

                // apply pending mesh data
                PendingMeshDataComponent pendingMeshData = entity.GetComponent<PendingMeshDataComponent>();
                renderedMeshComponent.VertexBuffer.SetBufferData(pendingMeshData.Vertices ?? new Vector3[0],
                    pendingMeshData.Colors ?? new Color64[0]);
                renderedMeshComponent.BufferObject.SetBufferData(pendingMeshData.Indices);
                renderedMeshComponent.VertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
                renderedMeshComponent.VertexArrayObject.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);

                // remove now processed mesh data component
                entityManager.RemoveComponent<PendingMeshDataComponent>(entity);
            }
        }
    }
}
