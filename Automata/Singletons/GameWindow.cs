#region

using System;
using System.Diagnostics;
using System.Numerics;
using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Singletons
{
    public class GameWindow : Singleton<GameWindow>
    {
        public new static void Validate()
        {
            if (Instance == null)
            {
                throw new InvalidOperationException($"Singleton '{nameof(GameWindow)}' has not been instantiated.");
            }
            else if (Instance.Window == null)
            {
                throw new InvalidOperationException($"Singleton '{nameof(GameWindow)}' does not have a valid '{nameof(IWindow)}' assigned.");
            }
        }

        private IWindow? _Window;
        private bool _WindowSet;

        public IWindow? Window
        {
            get
            {
                if (_Window == null)
                {
                    throw new NullReferenceException(nameof(Window));
                }
                else
                {
                    return _Window;
                }
            }
            set
            {
                if (_WindowSet)
                {
                    throw new InvalidOperationException($"'{nameof(Window)}' has already been set.");
                }
                else
                {
                    _Window = value;
                    _WindowSet = true;
                }
            }
        }

        public Vector2 Position
        {
            get
            {
                Debug.Assert(_Window != null);

                return new Vector2(_Window.Position.X, _Window.Position.Y);
            }
        }

        public Vector2 Size
        {
            get
            {
                Debug.Assert(_Window != null);

                return new Vector2(_Window.Size.Width, _Window.Size.Height);
            }
        }

        public GameWindow()
        {
            AssignSingletonInstance(this);
        }
    }
}
