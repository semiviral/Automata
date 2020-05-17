#region

using Automata.Input;
using Automata.Rendering;

#endregion

namespace Automata.Worlds
{
    public class GameWorld : World
    {
        public GameWorld(bool active) : base(active)
        {
            // input systems
            SystemManager.RegisterSystem<MovementSystem, FirstOrderSystem>(SystemRegistrationOrder.After);
            SystemManager.RegisterSystem<RotationSystem, FirstOrderSystem>(SystemRegistrationOrder.After);

            // rendering systems
            SystemManager.RegisterSystem<CameraMatrixSystem, RenderOrderSystem>(SystemRegistrationOrder.Before);
            SystemManager.RegisterSystem<RenderSystem, RenderOrderSystem>(SystemRegistrationOrder.After);
        }
    }
}
