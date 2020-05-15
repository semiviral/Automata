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
            SystemManager.RegisterSystem<MovementSystem, CameraRotationSystem>();

            // rendering systems
            SystemManager.RegisterSystem<CameraMatrixesSystem, RenderOrderSystem>();
            SystemManager.RegisterSystem<RenderSystem, CameraMatrixesSystem>();


            // cleanup systems
            SystemManager.RegisterSystem<ComponentChangedCleanupSystem, LastOrderSystem>();
        }
    }
}
