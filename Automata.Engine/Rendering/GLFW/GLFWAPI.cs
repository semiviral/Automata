using Silk.NET.GLFW;

namespace Automata.Engine.Rendering.GLFW
{
    public class GLFWAPI : Singleton<GLFWAPI>
    {
        public Glfw GLFW { get; }

        public GLFWAPI() => GLFW = Glfw.GetApi();
    }
}
