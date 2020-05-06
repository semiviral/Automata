#region

using Automata.Input;
using Automata.Rendering;

#endregion

namespace Automata.Core
{
    public class GameWorld : World
    {
        public GameWorld(bool active = false) : base(active)
        {
            SystemManager.RegisterSystem<InputSystem>(SystemManager.INPUT_SYSTEM_ORDER);
            SystemManager.RegisterSystem<RenderingSystem>(SystemManager.RENDER_SYSTEM_ORDER);
        }
    }
}
