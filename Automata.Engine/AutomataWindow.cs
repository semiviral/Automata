using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Automata.Engine.Collections;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.OpenGL;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using ErrorCode = Silk.NET.GLFW.ErrorCode;

namespace Automata.Engine
{
    public delegate void WindowResizedEventHandler(object sender, Vector2<int> newSize);

    public delegate void WindowFocusChangedEventHandler(object sender, bool isFocused);

    public delegate void WindowClosingEventHandler(object sender);

    public class AutomataWindow : Singleton<AutomataWindow>, IDisposable
    {
        private readonly APIVersion _PreferredOGLVersion = new APIVersion(4, 6);
        private readonly APIVersion _PreferredVulkanVersion = new APIVersion(1, 2);

        private IWindow? _Window;

        private IWindow Window
        {
            get
            {
                if (_Window is null)
                {
                    throw new InvalidOperationException("Window has not been created.");
                }
                else
                {
                    return _Window;
                }
            }
        }

        public TimeSpan VSyncFrameTime { get; private set; }
        public string Title { get => Window.Title; set => Window.Title = value; }
        public Vector2<int> Position { get => Window.Position.AsVector(); set => Window.Position = value.AsPoint(); }
        public Vector4 Viewport => new Vector4(0f, 0f, Window.Size.Width, Window.Size.Height);
        public float AspectRatio => (float)Window.Size.Width / (float)Window.Size.Height;

        public Vector2<int> Size { get => Window.Size.AsVector(); set => Window.Size = value.AsSize(); }

        public bool Focused { get; private set; }

        public Vector2<int> Center => Size / 2;

        public AutomataWindow() => Focused = true;

        public GL GetOpenGLContext() => GL.GetApi(Window.GLContext);

        public IVkSurface GetSurface() => Window.VkSurface ?? throw new NullReferenceException(nameof(Window.VkSurface));


        #region Creation

        public void CreateWindow(WindowOptions windowOptions, ContextAPI contextAPI)
        {
            IWindow construct_window_impl_impl(WindowOptions options)
            {
                options.API = contextAPI switch
                {
                    ContextAPI.OpenGL => new GraphicsAPI(contextAPI, ContextProfile.Core, ContextFlags.Debug, _PreferredOGLVersion),
                    ContextAPI.Vulkan => new GraphicsAPI(contextAPI, ContextProfile.Core, ContextFlags.Debug, _PreferredVulkanVersion),
                    _ => throw new NotSupportedException("Only OpenGL and Vulkan contexts are supported by Automata.")
                };

                IWindow window = Silk.NET.Windowing.Window.Create(options);
                window.Resize += OnWindowResized;
                window.FocusChanged += OnWindowFocusedChanged;
                window.Closing += OnWindowClosing;

                return window;
            }

            try
            {
                _Window = construct_window_impl_impl(windowOptions);
                _Window.Initialize();
            }
            catch (GlfwException glfw_exception) when (glfw_exception.ErrorCode is ErrorCode.VersionUnavailable)
            {
                throw new OpenGLVersionNotSupportedException(_PreferredOGLVersion);
            }

            InputManager.Instance.RegisterView(Window);

            DetermineVSyncRefreshRate();
        }

        private void DetermineVSyncRefreshRate()
        {
            const double default_refresh_rate = 60d;

            double refresh_rate = Window.Monitor?.VideoMode.RefreshRate.GetValueOrDefault() ?? default_refresh_rate;

            VSyncFrameTime = TimeSpan.FromSeconds(1d / refresh_rate);
            Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AutomataWindow), $"VSync framerate configured to {refresh_rate} FPS."));
        }

        #endregion Creation


        #region Runtime

        public async Task RunAsync()
        {
            try
            {
                Stopwatch delta_timer = new Stopwatch();
                BoundedConcurrentQueue<double> fps = new BoundedConcurrentQueue<double>(60);
                fps.Enqueue(0d); // so we don't get a 'Sequence contains no elements' exception.

                while (!Window.IsClosing)
                {
                    TimeSpan delta_time = delta_timer.Elapsed;

                    delta_timer.Restart();
                    Window.DoEvents();

                    if (InputManager.Instance.IsKeyPressed(Key.Escape))
                    {
                        Window.Close();
                    }

                    if (!Window.IsClosing)
                    {
                        await World.GlobalUpdateAsync(delta_time);
                        InputManager.Instance.CheckAndExecuteInputActions();

                        Window.DoEvents();
                        Window.SwapBuffers();
                    }

                    if (Window.VSync)
                    {
                        WaitForNextMonitorRefresh(delta_timer);
                    }

                    fps.Enqueue(1d / delta_timer.Elapsed.TotalSeconds);
                    Title = $"Automata {fps.Average():0.00} FPS";
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(_LogFormat, $"exception occured: {ex}\r\n{ex.StackTrace}"));
            }
            finally
            {
                Dispose();
            }
        }

        private void WaitForNextMonitorRefresh(Stopwatch deltaTimer) { SpinWait.SpinUntil(() => deltaTimer.Elapsed >= VSyncFrameTime); }

        #endregion Runtime


        #region Events

        public event WindowResizedEventHandler? Resized;

        public event WindowFocusChangedEventHandler? FocusChanged;

        public event WindowClosingEventHandler? Closing;

        private void OnWindowResized(Size size)
        {
            GLAPI.Instance.GL.Viewport(Window.Size);
            Resized?.Invoke(this, Size);
        }

        private void OnWindowFocusedChanged(bool focused)
        {
            Focused = focused;
            FocusChanged?.Invoke(this, focused);
        }

        private void OnWindowClosing()
        {
            Closing?.Invoke(this);

#if DEBUG
            if (OpenGLObject.ObjectsAlive.Count > 0)
            {
                Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, "OPENGL OBJECT TRACE",
                    $"{OpenGLObject.ObjectsAlive.Count} OF '{nameof(OpenGLObject)}' LEFT ALIVE AT PROGRAM EXIT:\r\n\t{string.Join("\r\n\t", OpenGLObject.ObjectsAlive.Values)}"));
            }
#endif
        }

        #endregion Events


        #region IDisposable

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            _Window?.Dispose();

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        ~AutomataWindow()
        {
            if (Disposed)
            {
                return;
            }

            _Window?.Dispose();
        }

        #endregion IDisposable
    }
}
