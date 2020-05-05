#region

using System;
using System.Numerics;
using Automata.Core;
using Automata.Rendering.OpenGL;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class MeshCompositionSystem : ComponentSystem
    {
        private readonly GL _GL;
        private readonly Shader _DefaultShader;

        public MeshCompositionSystem()
        {
            UtilizedComponentTypes = new[]
            {
                typeof(PendingMeshDataComponent)
            };

            _GL = GL.GetApi();
            _DefaultShader = new Shader(_GL);
        }

        public override void Update()
        {
            foreach (IEntity entity in EntityManager.GetEntitiesWithComponent<PendingMeshDataComponent>())
            {
                // create a shader component if one doesn't exist on object
                if (!entity.TryGetComponent(out RenderedShaderComponent _))
                {
                    EntityManager.RegisterComponent(entity, new RenderedShaderComponent
                    {
                        Shader = _DefaultShader
                    });
                }

                // create gpu buffers object if one doesn't exist on entity
                if (!entity.TryGetComponent(out GPUMeshComponent gpuMeshComponent))
                {
                    EntityManager.RegisterComponent(entity, gpuMeshComponent = new GPUMeshComponent
                    {
                        VertexBuffer = new VertexBuffer(_GL),
                        BufferObject = new BufferObject<uint>(_GL, BufferTargetARB.ElementArrayBuffer),
                        VertexArrayObject = new VertexArrayObject<float, uint>(_GL, gpuMeshComponent.VertexBuffer!, gpuMeshComponent.BufferObject!),
                    });
                }

                // null checks for C#8 null safety
                if (gpuMeshComponent.VertexBuffer == null)
                {
                    throw new NullReferenceException(nameof(gpuMeshComponent.VertexBuffer));
                }
                else if (gpuMeshComponent.BufferObject == null)
                {
                    throw new NullReferenceException(nameof(gpuMeshComponent.BufferObject));
                }
                else if (gpuMeshComponent.VertexArrayObject == null)
                {
                    throw new NullReferenceException(nameof(gpuMeshComponent.VertexArrayObject));
                }

                // apply pending mesh data
                PendingMeshDataComponent pendingMeshData = entity.GetComponent<PendingMeshDataComponent>();
                gpuMeshComponent.VertexBuffer.SetBufferData(pendingMeshData.Vertices ?? new Vector3[0], pendingMeshData.Colors ?? new Color64[0]);
                gpuMeshComponent.BufferObject.SetBufferData(pendingMeshData.Triangles);
                gpuMeshComponent.VertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
                gpuMeshComponent.VertexArrayObject.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);

                // remove now processed mesh data component
                entity.TryRemoveComponent<PendingMeshDataComponent>();
            }
        }
    }
}
