#region

using System.Collections.Generic;
using Automata.Core;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Input
{
    public class InputSystem : ComponentSystem
    {
        private readonly HashSet<Key> _KeysUp;
        private readonly HashSet<Key> _KeysDown;

        public InputSystem(IView window) : base(SystemManager.INPUT_SYSTEM_ORDER)
        {
            IInputContext inputContext = window.CreateInput();

            foreach (IKeyboard keyboard in inputContext.Keyboards)
            {
                keyboard.KeyUp += OnKeyUp;
                keyboard.KeyDown += OnKeyDown;
            }

            _KeysUp = new HashSet<Key>();
            _KeysDown = new HashSet<Key>();
        }

        public override void Update()
        {
            if ((_KeysUp.Count == 0) && (_KeysDown.Count == 0))
            {
                return;
            }

            foreach (KeyboardInputComponent inputComponent in EntityManager.GetComponents<KeyboardInputComponent>())
            {
                inputComponent.KeysUp.Clear();
                inputComponent.KeysDown.Clear();

                inputComponent.KeysUp.UnionWith(_KeysUp);
                inputComponent.KeysDown.UnionWith(_KeysDown);
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
