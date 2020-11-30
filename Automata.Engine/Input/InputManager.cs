using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Automata.Engine.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Automata.Engine.Input
{
    public delegate void KeyboardInputEventHandler(IKeyboard keyboard, Key key, int arg);

    public delegate void MouseInputEventHandler(IMouse mouse, MouseButton button);

    public delegate void MouseMovedEventHandler(IMouse mouse, Vector2 pointerPosition);

    public delegate void MouseScrolledEventHandler(IMouse mouse, Vector2 scrollPosition);

    public sealed class InputManager : Singleton<InputManager>
    {
        private class InputAction
        {
            public Key[] KeyCombination { get; }
            public Action Action { get; }
            public bool Activated { get; set; }

            public InputAction(Action action, params Key[] keyCombination) => (Action, KeyCombination) = (action, keyCombination);
        }

        private readonly List<IKeyboard> _Keyboards;
        private readonly List<IMouse> _Mice;
        private readonly List<InputAction> _InputActions;

        private IInputContext? _InputContext;

        public InputManager()
        {
            _Keyboards = new List<IKeyboard>();
            _Mice = new List<IMouse>();
            _InputActions = new List<InputAction>();
        }

        public void RegisterView(IView view)
        {
            _InputContext = view.CreateInput();

            if (_InputContext is null)
            {
                throw new NullReferenceException("View provided a null input context.");
            }

            foreach (IKeyboard keyboard in _InputContext.Keyboards)
            {
                keyboard.KeyUp += OnKeyUp;
                keyboard.KeyDown += OnKeyDown;

                _Keyboards.Add(keyboard);
            }

            foreach (IMouse mouse in _InputContext.Mice)
            {
                mouse.Cursor.CursorMode = CursorMode.Hidden;
                mouse.MouseDown += OnMouseButtonDown;
                mouse.MouseUp += OnMouseButtonUp;
                mouse.MouseMove += OnMouseMoved;
                mouse.Scroll += OnMouseScrolled;

                _Mice.Add(mouse);
            }
        }

        public void RegisterInputAction(Action action, params Key[] keys) => _InputActions.Add(new InputAction(action, keys));

        public bool IsKeyPressed(Key key) => _Keyboards.Any(keyboard => keyboard.IsKeyPressed(key));
        public bool IsButtonPressed(MouseButton mouseButton) => _Mice.Any(mouse => mouse.IsButtonPressed(mouseButton));

        public Vector2<float> GetMousePosition(int mouseIndex)
        {
            if ((mouseIndex < 0) || (mouseIndex >= _Mice.Count))
            {
                throw new IndexOutOfRangeException(nameof(mouseIndex));
            }

            PointF position = _Mice[mouseIndex].Position;
            position.Y = -position.Y + AutomataWindow.Instance.Size.Y;

            return position.AsVector();
        }

        /// <summary>
        ///     Returns mouse position relative to the center of the window.
        /// </summary>
        /// <param name="mouseIndex"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Vector2<float> GetMousePositionCenterRelative(int mouseIndex) =>
            GetMousePosition(mouseIndex) - AutomataWindow.Instance.Center.Convert<float>();

        public void SetMousePositionCenterRelative(int mouseIndex, Vector2<float> position)
        {
            if ((mouseIndex < 0) || (mouseIndex >= _Mice.Count))
            {
                throw new IndexOutOfRangeException(nameof(mouseIndex));
            }

            position += AutomataWindow.Instance.Center.Convert<float>();
            _Mice[mouseIndex].Position = position.AsPointF();
        }

        public void CheckAndExecuteInputActions()
        {
            foreach (InputAction inputAction in _InputActions)
            {
                if (inputAction.KeyCombination.All(IsKeyPressed))
                {
                    if (inputAction.Activated)
                    {
                        continue;
                    }

                    inputAction.Activated = true;
                    inputAction.Action.Invoke();
                }
                else
                {
                    inputAction.Activated = false;
                }
            }
        }


        #region Keyboard Events

        public event KeyboardInputEventHandler? KeyUp;
        public event KeyboardInputEventHandler? KeyDown;

        private void OnKeyUp(IKeyboard keyboard, Key key, int arg) { KeyUp?.Invoke(keyboard, key, arg); }

        private void OnKeyDown(IKeyboard keyboard, Key key, int arg) { KeyDown?.Invoke(keyboard, key, arg); }

        #endregion


        #region Mouse Events

        public event MouseInputEventHandler? MouseButtonDown;
        public event MouseInputEventHandler? MouseButtonUp;
        public event MouseMovedEventHandler? MouseMoved;
        public event MouseScrolledEventHandler? MouseScrolled;

        private void OnMouseButtonDown(IMouse mouse, MouseButton button) { MouseButtonDown?.Invoke(mouse, button); }

        private void OnMouseButtonUp(IMouse mouse, MouseButton button) { MouseButtonUp?.Invoke(mouse, button); }

        private void OnMouseMoved(IMouse mouse, PointF point) { MouseMoved?.Invoke(mouse, new Vector2(point.X, point.Y)); }

        private void OnMouseScrolled(IMouse mouse, ScrollWheel scrollWheel) { MouseScrolled?.Invoke(mouse, new Vector2(scrollWheel.X, scrollWheel.Y)); }

        #endregion
    }
}
