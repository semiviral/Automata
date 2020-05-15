#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Core;
using Automata.Core.Systems;
using Automata.Rendering;
using Silk.NET.OpenGL;

#endregion

namespace AutomataTest
{
    /// <summary>
    ///     Consumes a <see cref="PendingMesh{T}" /> and creates relevant GPU buffers so a given mesh can be
    ///     rendered.
    /// </summary>
    public class MeshCompositionSystem : ComponentSystem
    {
        private readonly Stack<IEntity> _RemovePendingMeshDataEntities;

        public MeshCompositionSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(PendingMesh<int>)
            };

            _RemovePendingMeshDataEntities = new Stack<IEntity>();
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<PendingMesh<int>>())
            {
                // create gpu buffers object if one doesn't exist on entity


                // apply pending mesh data


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

                entityManager.RemoveComponent<PendingMesh<int>>(entity);
            }
        }
    }
}
