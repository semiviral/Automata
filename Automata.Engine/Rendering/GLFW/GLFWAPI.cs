#region

using Silk.NET.GLFW;

#endregion


namespace Automata.Engine.Rendering.GLFW
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
