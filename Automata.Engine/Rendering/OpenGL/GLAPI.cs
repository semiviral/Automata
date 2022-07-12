using System;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Automata.Engine.Rendering.OpenGL
{
    public class GLAPI : Singleton<GLAPI>
    {
        public GL GL { get; }

        public unsafe GLAPI()
        {
            // validate dependency or throw
            GL = AutomataWindow.Instance.GetOpenGLContext();

            string version = SilkMarshal.PtrToString((nint)GL.GetString(StringName.Version));
            Log.Information(string.Format(_LogFormat, $"OpenGL version {version}"));

            // configure debug callback
            GL.GetInteger(GetPName.ContextFlags, out int flags);

            if (!((ContextFlags)flags).HasFlag(ContextFlags.Debug))
            {
                return;
            }

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(DebugOutputCallback, (void*)null!);
            GL.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, (uint*)null!, true);
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
        ///         only be used when that functionality is insufficient or failing.
        ///     </p>
        /// </remarks>
        /// <param name="checkForErrors"></param>
        /// <exception cref="OpenGLException"></exception>
        public static void CheckForErrorsAndThrow(bool checkForErrors)
        {
            if (!checkForErrors)
            {
                return;
            }

            GLEnum gl_error = Instance.GL.GetError();

            switch (gl_error)
            {
                case GLEnum.NoError: break;
                default: throw new OpenGLException(gl_error);
            }
        }

        public static void UnbindProgramPipeline() => Instance.GL.BindProgramPipeline(0);
        public static void UnbindVertexArray() => Instance.GL.BindVertexArray(0);
        public static void UnbindBuffer(BufferTargetARB target) => Instance.GL.BindBuffer(target, 0);

        private static void DebugOutputCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint messagePtr, nint userParamPtr)
        {
            static void log_ogl_debug_message_impl_impl(DebugSource debugSource, DebugType debugType, DebugSeverity debugSeverity, string message)
            {
                const string ogl_log_format = "[DEBUG {0}] {1}: {2}";

                string log_string = string.Format(ogl_log_format, debugSource, debugType, message);

                switch (debugSeverity)
                {
                    case DebugSeverity.DontCare:
                        Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(GLAPI), log_string));
                        break;
                    case DebugSeverity.DebugSeverityNotification:
                        Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(GLAPI), log_string));
                        break;
                    case DebugSeverity.DebugSeverityLow:
                        Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(GLAPI), log_string));
                        break;
                    case DebugSeverity.DebugSeverityMedium:
                        Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(GLAPI), log_string));
                        break;
                    case DebugSeverity.DebugSeverityHigh:
                        Log.Fatal(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(GLAPI), log_string));
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(debugSeverity), debugSeverity, null);
                }
            }

            log_ogl_debug_message_impl_impl((DebugSource)source, (DebugType)type, (DebugSeverity)severity, SilkMarshal.PtrToString(messagePtr));
        }
    }
}
