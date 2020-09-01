#region

using Automata.Numerics;

#endregion

namespace Automata.Rendering.GLFW
{
    public delegate void WindowResizedEventHandler(object sender, Vector2i newSize);

    public delegate void WindowFocusChangedEventHandler(object sender, bool isFocused);

    public delegate void WindowClosingEventHandler(object sender);
}
