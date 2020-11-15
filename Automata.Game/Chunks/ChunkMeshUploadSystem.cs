using System;
using System.Threading.Tasks;
using Automata.Engine.Entities;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Systems;

namespace Automata.Game.Chunks
{
    public class ChunkMeshUploadSystem : ComponentSystem
    {
        private MultiDrawIndirectMesh? _MultiDrawIndirectMesh;

        public override void Registered(EntityManager entityManager)
        {
            const uint one_kb = 1024u;
            const uint one_mb = one_kb * one_kb;
            const uint one_gb = one_kb * one_kb * one_kb;

            _MultiDrawIndirectMesh = new MultiDrawIndirectMesh(GLAPI.Instance.GL, 3u * one_mb, one_gb);

            entityManager.RegisterEntity(new Entity
            {
                new RenderMesh
                {
                    Mesh = _MultiDrawIndirectMesh
                }
            });
        }

        [HandledComponents(DistinctionStrategy.All, typeof(Chunk), typeof(RenderMesh)),
         HandledComponents(DistinctionStrategy.All, typeof(Chunk), typeof(RenderModel))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {


            return ValueTask.CompletedTask;
        }
    }
}
