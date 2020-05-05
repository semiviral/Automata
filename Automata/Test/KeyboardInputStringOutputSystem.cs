#region

using Automata.Core;
using Automata.Input;
using Serilog;

#endregion

namespace Automata.Test
{
    public class KeyboardInputStringOutputSystem : ComponentSystem
    {
        public override void Update()
        {
            foreach (KeyboardInputComponent inputComponent in EntityManager.GetComponents<KeyboardInputComponent>())
            {
                if (inputComponent.KeysDown.Count == 0)
                {
                    return;
                }

                Log.Information($"{string.Join(", ", inputComponent.KeysDown)}");
            }
        }
    }
}
