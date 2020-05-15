#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Core;
using Automata.Core.Systems;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    /// <summary>
    ///     Consumes a <see cref="PendingMesh{T}" /> and creates relevant GPU buffers so a given mesh can be
    ///     rendered.
    /// </summary>
    public class UnpackedMeshCompositionSystem : ComponentSystem
    {
        private readonly Stack<IEntity> _RemovePendingMeshDataEntities;

        public UnpackedMeshCompositionSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(PendingMesh<float>)
            };

            _RemovePendingMeshDataEntities = new Stack<IEntity>();
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<PendingMesh<float>>())
            {
                // create gpu buffers object if one doesn't exist on entity
                if (!entity.TryGetComponent(out Mesh mesh))
                {
                    mesh = new Mesh();

                    entityManager.RegisterComponent(entity, mesh);
                }
                else if (mesh.VertexesBuffer == null)
                {
                    throw new NullReferenceException(nameof(mesh.VertexesBuffer));
                }
                else if (mesh.IndexesBuffer == null)
                {
                    throw new NullReferenceException(nameof(mesh.IndexesBuffer));
                }
                else if (mesh.VertexArrayObject == null)
                {
                    throw new NullReferenceException(nameof(mesh.VertexArrayObject));
                }

                // apply pending mesh data
                PendingMesh<float> pendingMesh = entity.GetComponent<PendingMesh<float>>();
                mesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes.ToArray());
                mesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes.ToArray());
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

                entityManager.RemoveComponent<PendingMesh<float>>(entity);
            }
        }
    }
}
