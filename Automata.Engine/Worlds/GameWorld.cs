#region

using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Systems;

#endregion

namespace Automata.Engine.Worlds
{
    public class GameWorld : World
    {
        public GameWorld(bool active) : base(active)
        {
            // input systems
            SystemManager.RegisterSystem<KeyboardMovementSystem, FirstOrderSystem>(SystemRegistrationOrder.After);
            SystemManager.RegisterSystem<MouseRotationSystem, FirstOrderSystem>(SystemRegistrationOrder.After);

            // rendering systems
            SystemManager.RegisterSystem<CameraMatrixSystem, RenderOrderSystem>(SystemRegistrationOrder.Before);
            SystemManager.RegisterSystem<RenderSystem, RenderOrderSystem>(SystemRegistrationOrder.After);
        }
    }
}
