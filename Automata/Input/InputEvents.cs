#region

using System.Numerics;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Input
{
    public delegate void KeyboardInputEventHandler(IKeyboard keyboard, Key key, int arg);

    public delegate void MouseInputEventHandler(IMouse mouse, MouseButton button);

    public delegate void MouseMovedEventHandler(IMouse mouse, Vector2 pointerPosition);

    public delegate void MouseScrolledEventHandler(IMouse mouse, Vector2 scrollPosition);
}
