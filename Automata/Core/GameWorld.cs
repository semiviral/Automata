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
            SystemManager.RegisterSystem<RotationSystem, FirstOrderSystem>();

            // meshing systems
            SystemManager.RegisterSystem<MeshCompositionSystem, DefaultOrderSystem>();

            // rendering systems
            SystemManager.RegisterSystem<PreRenderSystem, RenderOrderSystem>();
            SystemManager.RegisterSystem<RenderSystem, PreRenderSystem>();

            // cleanup systems
            SystemManager.RegisterSystem<ComponentChangedCleanupSystem, LastOrderSystem>();
        }
    }
}
