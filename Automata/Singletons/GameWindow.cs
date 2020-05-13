#region

using System;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Singletons
{
    public class GameWindow : Singleton<GameWindow>
    {
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
    }
}
