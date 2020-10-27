#region

using System;
using Automata.Engine.Rendering.GLFW;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class GLAPI : Singleton<GLAPI>
    {
        public GL GL { get; }

        public unsafe GLAPI()
        {
            AssignSingletonInstance(this);

            GL = GL.GetApi(AutomataWindow.Instance.GLContext);

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(DebugOutputCallback, null);
            GL.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, (uint*)null!, true);
        }

        public void CheckForErrorsAndThrow()
        {
            //GLEnum glError = GL.GetError();

            //switch (glError)
            //{
            //    case GLEnum.NoError: break;
            //    default: throw new OpenGLException(glError);
            //}
        }

        public static void DebugOutputCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr messagePtr, IntPtr userParamPtr)
        {
            string message = SilkMarshal.MarshalPtrToString(messagePtr);

            Console.WriteLine($"Output {source} {type} {message}");
        }
    }
}
