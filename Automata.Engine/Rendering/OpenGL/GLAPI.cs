#region

using System;
using Automata.Engine.Rendering.GLFW;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class GLAPI : Singleton<GLAPI>
    {
        public GL GL { get; }

        public unsafe GLAPI()
        {
            AutomataWindow.Validate(); // validate dependency or throw
            GL = AutomataWindow.Instance.GetGL();

            string version = GL.GetString(StringName.Version);
            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(GLAPI), $"OpenGL version {version}"));

            // configure debug callback
            GL.GetInteger(GetPName.ContextFlags, out int flags);

            if (((ContextFlags)flags).HasFlag(ContextFlags.Debug))
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.DebugOutputSynchronous);
                GL.DebugMessageCallback(DebugOutputCallback, (void*)null!);
                GL.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, (uint*)null!, true);
            }

            AssignSingletonInstance(this);
        }

        /// <summary>
        ///     Manually query OpenGL for errors.
        /// </summary>
        /// <remarks>
        ///     <p>
        ///         This function is very simple, and offers little-to-no debug information.
        ///     </p>
        ///     <p>
        ///         Thus, try to rely on the default debug context exception handling. This should
        ///         only be used when that functionality is failing.
        ///     </p>
        /// </remarks>
        /// <param name="checkForErrors"></param>
        /// <exception cref="OpenGLException"></exception>
        public void CheckForErrorsAndThrow(bool checkForErrors)
        {
            if (!checkForErrors) return;

            GLEnum glError = GL.GetError();

            switch (glError)
            {
                case GLEnum.NoError: break;
                default: throw new OpenGLException(glError);
            }
        }

        private static void DebugOutputCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr messagePtr, IntPtr userParamPtr)
        {
            string message = SilkMarshal.MarshalPtrToString(messagePtr);

            Console.WriteLine($"Output {source} {type} {message}");
        }
    }
}
