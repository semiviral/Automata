using System;
using System.Threading.Tasks;
using Automata.Engine.Entities;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Systems;

namespace Automata.Game.Chunks
{
    public class ChunkMeshUploadSystem : ComponentSystem
    {
        [HandledComponents(DistinctionStrategy.All, typeof(Chunk), typeof(RenderMesh)),
         HandledComponents(DistinctionStrategy.All, typeof(Chunk), typeof(RenderModel))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta) => base.Update(entityManager, delta);
    }
}
