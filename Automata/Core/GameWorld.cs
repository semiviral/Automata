#region

using Automata.Core.Systems;
using Automata.Rendering;

#endregion

namespace Automata.Core
{
    public class GameWorld : World
    {
        public GameWorld(bool active) : base(active)
        {
            // input systems
            SystemManager.RegisterSystem<CameraRotationSystem>(SystemManager.INPUT_SYSTEM_ORDER);

            // meshing systems
            SystemManager.RegisterSystem<MeshCompositionSystem>(SystemManager.MESH_COMPOSITION_SYSTEM_ORDER);

            // rendering systems
            SystemManager.RegisterSystem<RenderingSystem>(SystemManager.RENDER_SYSTEM_ORDER);

            // cleanup systems
            SystemManager.RegisterSystem<TranslationChangedCleanupSystem>(SystemManager.FINAL_SYSTEM_ORDER);
            SystemManager.RegisterSystem<RotationChangedCleanupSystem>(SystemManager.FINAL_SYSTEM_ORDER);
        }
    }
}
