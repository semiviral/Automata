#region

using Automata.Engine.Numerics;

#endregion


namespace Automata.Engine.Rendering.GLFW
{
    public delegate void WindowResizedEventHandler(object sender, Vector2i newSize);

    public delegate void WindowFocusChangedEventHandler(object sender, bool isFocused);

    public delegate void WindowClosingEventHandler(object sender);
}
