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
            SystemManager.RegisterSystem<KeyboardMovementSystem, FirstOrderSystem>(SystemRegistrationOrder.Before);
            SystemManager.RegisterSystem<MouseRotationSystem, FirstOrderSystem>(SystemRegistrationOrder.Before);
            SystemManager.RegisterSystem<RenderSystem, LastOrderSystem>(SystemRegistrationOrder.Before);
        }
    }
}
