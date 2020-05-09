#region

using Automata.Core;
using Automata.Input;
using Serilog;

#endregion

namespace Automata.Test
{
    public class KeyboardInputStringOutputSystem : ComponentSystem
    {
        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (KeyboardInput inputComponent in entityManager.GetComponents<KeyboardInput>())
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
