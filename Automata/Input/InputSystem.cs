#region

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Automata.Core;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Input
{
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

            HandledComponentTypes = new[]
            {
                typeof(UnhandledInputContext),
                typeof(KeyboardInput)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            List<IEntity> registeredInputContextEntities = entityManager.GetEntitiesWithComponents<UnhandledInputContext>().ToList();

            foreach (IEntity entity in registeredInputContextEntities)
            {
                if (entity == null)
                {
                    continue;
                }

                UnhandledInputContext unhandledInputContext = entity.GetComponent<UnhandledInputContext>();

                if (unhandledInputContext.InputContext != null)
                {
                    RegisterInputContext(unhandledInputContext.InputContext);
                }

                entityManager.RemoveComponent<UnhandledInputContext>(entity);
            }

            if ((_KeysUp.Count == 0) && (_KeysDown.Count == 0))
            {
                return;
            }

            foreach (KeyboardInput inputComponent in entityManager.GetComponents<KeyboardInput>())
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

            foreach (IMouse mouse in inputContext.Mice)
            {
                mouse.MouseDown += OnMouseButtonDown;
                mouse.MouseUp += OnMouseButtonUp;
                mouse.MouseMove += OnMouseMoved;
                mouse.Scroll += OnMouseScrolled;
            }
        }

        #region Keyboard Events

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


        #region Mouse Events

        public event MouseInputEventHandler? MouseButtonDown;
        public event MouseInputEventHandler? MouseButtonUp;
        public event MouseMovedEventHandler? MouseMoved;
        public event MouseScrolledEventHandler? MouseScrolled;

        private void OnMouseButtonDown(IMouse mouse, MouseButton button)
        {
            MouseButtonDown?.Invoke(mouse, button);
        }

        private void OnMouseButtonUp(IMouse mouse, MouseButton button)
        {
            MouseButtonUp?.Invoke(mouse, button);
        }

        private void OnMouseMoved(IMouse mouse, PointF point)
        {
            MouseMoved?.Invoke(mouse, new Vector2(point.X, point.Y));
        }

        private void OnMouseScrolled(IMouse mouse, ScrollWheel scrollWheel)
        {
            MouseScrolled?.Invoke(mouse, new Vector2(scrollWheel.X, scrollWheel.Y));
        }

        #endregion
    }
}
