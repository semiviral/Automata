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
            SystemManager.RegisterSystem<MovementSystem, FirstOrderSystem>();

            // meshing systems
            SystemManager.RegisterSystem<MeshCompositionSystem, DefaultOrderSystem>();

            // rendering systems
            SystemManager.RegisterSystem<RenderSystem, RenderOrderSystem>();
            SystemManager.RegisterSystem<CameraMatrixesSystem, RenderOrderSystem>();


            // cleanup systems
            SystemManager.RegisterSystem<ComponentChangedCleanupSystem, LastOrderSystem>();
        }
    }
}
