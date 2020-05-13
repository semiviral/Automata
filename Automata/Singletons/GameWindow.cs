#region

using System;
using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Singletons
{
    public class GameWindow : Singleton<GameWindow>
    {
        public static bool Validate()
        {
            if (Instance == null)
            {
                throw new InvalidOperationException($"Singleton '{nameof(GameWindow)}' has not been instantiated.");
            }
            else if (Instance.Window == null)
            {
                throw new InvalidOperationException($"Singleton '{nameof(GameWindow)}' does not have a valid '{nameof(IWindow)}' assigned.");
            }
            else
            {
                return true;
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

        public GameWindow()
        {
            AssignSingletonInstance(this);
        }
    }
}
