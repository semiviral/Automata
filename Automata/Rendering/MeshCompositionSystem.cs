#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Automata.Core;
using Automata.Core.Systems;
using Automata.Rendering.OpenGL;
using Automata.Singletons;
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

        private readonly Stack<IEntity> _RemovePendingMeshDataEntities;

        public MeshCompositionSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(PendingMeshDataComponent)
            };


            GLAPI.Validate();

            _GL = GLAPI.Instance.GL;
            _RemovePendingMeshDataEntities = new Stack<IEntity>();
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<PendingMeshDataComponent>())
            {
                // create gpu buffers object if one doesn't exist on entity
                if (!entity.TryGetComponent(out Mesh mesh))
                {
                    mesh = new Mesh
                    {
                        VertexBuffer = new BufferObject<float>(_GL, BufferTargetARB.ArrayBuffer),
                        IndicesBuffer = new BufferObject<uint>(_GL, BufferTargetARB.ElementArrayBuffer),
                    };
                    mesh.VertexArrayObject = new VertexArrayObject<float, uint>(_GL, mesh.VertexBuffer, mesh.IndicesBuffer);

                    entityManager.RegisterComponent(entity, mesh);
                }
                else if (mesh.VertexBuffer == null)
                {
                    throw new NullReferenceException(nameof(mesh.VertexBuffer));
                }
                else if (mesh.IndicesBuffer == null)
                {
                    throw new NullReferenceException(nameof(mesh.IndicesBuffer));
                }
                else if (mesh.VertexArrayObject == null)
                {
                    throw new NullReferenceException(nameof(mesh.VertexArrayObject));
                }

                // apply pending mesh data
                PendingMeshDataComponent pendingMeshData = entity.GetComponent<PendingMeshDataComponent>();
                mesh.VertexBuffer.SetBufferData(UnrollVertices(pendingMeshData.Vertices).ToArray());
                mesh.IndicesBuffer.SetBufferData(pendingMeshData.Indices);
                mesh.VertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 3, 0);

                // push entity for component removal
                _RemovePendingMeshDataEntities.Push(entity);
            }

            // remove now processed mesh data components
            while (_RemovePendingMeshDataEntities.TryPop(out IEntity? entity))
            {
                if (entity == null)
                {
                    continue;
                }

                entityManager.RemoveComponent<PendingMeshDataComponent>(entity);
            }
        }

        private static IEnumerable<float> UnrollVertices(IEnumerable<Vector3> vertices)
        {
            foreach (Vector3 vertex in vertices)
            {
                yield return vertex.X;
                yield return vertex.Y;
                yield return vertex.Z;
            }
        }
    }
}
