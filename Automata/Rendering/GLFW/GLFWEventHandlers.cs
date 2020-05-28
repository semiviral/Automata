using Automata.Numerics;
using Silk.NET.Vulkan;

namespace Automata.Rendering.GLFW
{
    public delegate void WindowResizedEventHandler(object sender, Vector2i newSize);

    public delegate void WindowFocusChangedEventHandler(object sender, bool isFocused);

    public delegate void WindowClosingEventHandler(object sender);
}
