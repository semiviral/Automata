#region

using Silk.NET.OpenGL;

#endregion

namespace Automata.Singletons
{
    public class GLAPI : Singleton<GLAPI>
    {
        public GL GL { get; }

        public GLAPI() => GL = GL.GetApi();
    }
}
