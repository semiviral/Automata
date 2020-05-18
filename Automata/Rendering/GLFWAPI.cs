#region

using Silk.NET.GLFW;

#endregion

namespace Automata.Rendering
{
    public class GLFWAPI : Singleton<GLFWAPI>
    {
        public Glfw GLFW { get; }

        public GLFWAPI()
        {
            AssignSingletonInstance(this);

            GLFW = Glfw.GetApi();
        }
    }
}
