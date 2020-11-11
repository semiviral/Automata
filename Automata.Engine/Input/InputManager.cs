#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.Windowing.Common;

#endregion


namespace Automata.Engine.Input
{
    public delegate void KeyboardInputEventHandler(IKeyboard keyboard, Key key, int arg);

    public delegate void MouseInputEventHandler(IMouse mouse, MouseButton button);

    public delegate void MouseMovedEventHandler(IMouse mouse, Vector2 pointerPosition);

    public delegate void MouseScrolledEventHandler(IMouse mouse, Vector2 scrollPosition);

    public sealed class InputManager : Singleton<InputManager>
    {
        private readonly List<IKeyboard> _Keyboards;
        private readonly List<IMouse> _Mice;

        private IInputContext? _InputContext;

        public InputManager()
        {
            _Keyboards = new List<IKeyboard>();
            _Mice = new List<IMouse>();
        }

        public void RegisterView(IView view)
        {
            _InputContext = view.CreateInput();

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

        public bool IsKeyPressed(Key key) => _Keyboards.Any(keyboard => keyboard.IsKeyPressed(key));
        public bool IsButtonPressed(MouseButton mouseButton) => _Mice.Any(mouse => mouse.IsButtonPressed(mouseButton));

        public Vector2 GetMousePosition(int mouseIndex)
        {
            if ((mouseIndex < 0) || (mouseIndex >= _Mice.Count)) throw new IndexOutOfRangeException(nameof(mouseIndex));

            PointF position = _Mice[mouseIndex].Position;
            position.Y = -position.Y + AutomataWindow.Instance.Size.Y;

            return Unsafe.As<PointF, Vector2>(ref position);
        }

        /// <summary>
        ///     Returns mouse position relative to the center of the window.
        /// </summary>
        /// <param name="mouseIndex"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Vector2 GetMousePositionCenterRelative(int mouseIndex) => GetMousePosition(mouseIndex) - (Vector2)AutomataWindow.Instance.Center;

        public void SetMousePositionCenterRelative(int mouseIndex, Vector2 position)
        {
            if ((mouseIndex < 0) || (mouseIndex >= _Mice.Count)) throw new IndexOutOfRangeException(nameof(mouseIndex));

            position += (Vector2)AutomataWindow.Instance.Center;
            _Mice[mouseIndex].Position = Unsafe.As<Vector2, PointF>(ref position);
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
