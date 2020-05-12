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
            SystemManager.RegisterSystem<CameraRotationSystem, FirstOrderSystem>();

            // meshing systems
            SystemManager.RegisterSystem<MeshCompositionSystem, DefaultOrderSystem>();

            // rendering systems
            SystemManager.RegisterSystem<RenderingSystem, RenderOrderSystem>();

            // cleanup systems
            SystemManager.RegisterSystem<TranslationChangedCleanupSystem, LastOrderSystem>();
            SystemManager.RegisterSystem<RotationChangedCleanupSystem, LastOrderSystem>();
        }
    }
}
