using Automata.Core;

namespace Automata.Rendering
{
    public class MeshRenderingSystem : ComponentSystem
    {
        public override void Registered()
        {

        }

        public override void Update()
        {
            foreach (IEntity entity in EntityManager.GetEntitiesWithComponent<DirtyMeshComponent>())
            {
                if (!entity.TryGetComponent(out RenderedShaderComponent renderedShaderComponent))
                {
                    // apply default shader
                }

                // get shader

                // create buffers if one doesn't exist
                // apply new buffer data

                entity.TryRemoveComponent<DirtyMeshComponent>();
            }
        }
    }
}
