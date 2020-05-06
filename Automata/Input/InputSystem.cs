#region

using System.Collections.Generic;
using System.Linq;
using Automata.Core;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Input
{
    public delegate void KeyboardInputEventHandler(IKeyboard keyboard, Key key, int arg);

    /// <summary>
    ///     System used for capturing and dispatching input updates.
    /// </summary>
    public class InputSystem : ComponentSystem
    {
        private readonly HashSet<Key> _KeysUp;
        private readonly HashSet<Key> _KeysDown;

        public InputSystem()
        {
            _KeysUp = new HashSet<Key>();
            _KeysDown = new HashSet<Key>();

            UtilizedComponentTypes = new[]
            {
                typeof(UnregisteredInputContextComponent),
                typeof(KeyboardInputComponent)
            };
        }

        public override void Update(EntityManager entityManager, double deltaTime)
        {
            List<IEntity> registeredInputContextEntities = entityManager.GetEntitiesWithComponents<UnregisteredInputContextComponent>().ToList();

            foreach (IEntity entity in registeredInputContextEntities)
            {
                if (entity == null)
                {
                    continue;
                }

                UnregisteredInputContextComponent unregisteredInputContextComponent = entity.GetComponent<UnregisteredInputContextComponent>();

                RegisterInputContext(unregisteredInputContextComponent.InputContext);

                entityManager.RemoveComponent<UnregisteredInputContextComponent>(entity);
            }

            if ((_KeysUp.Count == 0) && (_KeysDown.Count == 0))
            {
                return;
            }

            foreach (KeyboardInputComponent inputComponent in entityManager.GetComponents<KeyboardInputComponent>())
            {
                inputComponent.KeysUp.Clear();
                inputComponent.KeysDown.Clear();

                inputComponent.KeysUp.UnionWith(_KeysUp);
                inputComponent.KeysDown.UnionWith(_KeysDown);
            }
        }

        private void RegisterInputContext(IInputContext inputContext)
        {
            foreach (IKeyboard keyboard in inputContext.Keyboards)
            {
                keyboard.KeyUp += OnKeyUp;
                keyboard.KeyDown += OnKeyDown;
            }
        }

        #region Events

        public event KeyboardInputEventHandler? KeyUp;
        public event KeyboardInputEventHandler? KeyDown;

        private void OnKeyUp(IKeyboard keyboard, Key key, int arg)
        {
            _KeysUp.Add(key);
            _KeysDown.Remove(key);

            KeyUp?.Invoke(keyboard, key, arg);
        }

        private void OnKeyDown(IKeyboard keyboard, Key key, int arg)
        {
            _KeysDown.Add(key);

            KeyDown?.Invoke(keyboard, key, arg);
        }

        #endregion
    }
}
