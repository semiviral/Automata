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
            // input systems
            SystemManager.RegisterSystem<InputSystem>(SystemManager.INPUT_SYSTEM_ORDER);
            SystemManager.RegisterSystem<KeyboardInputTranslationSystem>(SystemManager.INPUT_SYSTEM_ORDER);
            SystemManager.RegisterSystem<KeyboardInputTranslationToTranslationSystem>(SystemManager.INPUT_SYSTEM_ORDER);

            // rendering systems
            SystemManager.RegisterSystem<MeshCompositionSystem>(SystemManager.RENDER_SYSTEM_ORDER);
            SystemManager.RegisterSystem<RenderingSystem>(SystemManager.RENDER_SYSTEM_ORDER);

            // cleanup systems
            SystemManager.RegisterSystem<TranslationChangedCleanupSystem>(SystemManager.FINAL_SYSTEM_ORDER);
        }
    }
}
