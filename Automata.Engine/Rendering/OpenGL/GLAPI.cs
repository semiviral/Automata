#region

using Automata.Engine.Rendering.GLFW;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class GLAPI : Singleton<GLAPI>
    {
        public static class StaticCube
        {
        // @formatter:off

        public static float[] Vertexes =
        {
            0f, 1f, 1f,
            0f, 0f, 1f,
            1f, 0f, 1f,
            1f, 1f, 1f,

            1f, 1f, 1f,
            1f, 0f, 1f,
            1f, 0f, 0f,
            1f, 1f, 0f,

            1f, 1f, 0f,
            1f, 0f, 0f,
            0f, 0f, 0f,
            0f, 1f, 0f,

            0f, 1f, 0f,
            0f, 0f, 0f,
            0f, 0f, 1f,
            0f, 1f, 1f,

            0f, 1f, 1f,
            1f, 1f, 1f,
            1f, 1f, 0f,
            0f, 1f, 0f,

            1f, 0f, 1f,
            0f, 0f, 1f,
            0f, 0f, 0f,
            1f, 0f, 0f
        };

        public static uint[] Indexes =
        {
            00, 01, 03,
            01, 02, 03,

            04, 05, 07,
            05, 06, 07,

            08, 09, 11,
            09, 10, 11,

            12, 13, 15,
            13, 14, 15,

            16, 17, 19,
            17, 18, 19,

            20, 21, 23,
            21, 22, 23
        };

            // @formatter:on
        }

        public GL GL { get; }

        public GLAPI()
        {
            AssignSingletonInstance(this);

            GL = GL.GetApi(AutomataWindow.Instance.GLContext);
        }
    }
}
