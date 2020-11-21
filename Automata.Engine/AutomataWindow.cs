using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Automata.Engine.Collections;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.OpenGL;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;
using ErrorCode = Silk.NET.GLFW.ErrorCode;

namespace Automata.Engine
{
    public delegate void WindowResizedEventHandler(object sender, Vector2i newSize);

    public delegate void WindowFocusChangedEventHandler(object sender, bool isFocused);

    public delegate void WindowClosingEventHandler(object sender);

    public class AutomataWindow : Singleton<AutomataWindow>, IDisposable
    {
        private readonly APIVersion _PreferredOGLVersion = new APIVersion(4, 6);
        private readonly APIVersion _PreferredVulkanVersion = new APIVersion(1, 2);

        private TimeSpan _MinimumFrameTime;

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

        public string Title { get => Window.Title; set => Window.Title = value; }
        public Vector2i Size { get => (Vector2i)Window.Size; set => Window.Size = (Size)value; }
        public Vector2i Position { get => (Vector2i)Window.Position; set => Window.Position = (Point)value; }
        public Vector2i Center => Size / 2;

        public bool Focused { get; private set; }

        public AutomataWindow() => Focused = true;

        public GL GetOpenGLContext() => GL.GetApi(Window.GLContext);

        public IVkSurface GetSurface() => Window.VkSurface ?? throw new NullReferenceException(nameof(Window.VkSurface));


        #region Creation

        public void CreateWindow(WindowOptions windowOptions, ContextAPI contextAPI)
        {
            IWindow ConstructWindowImpl(WindowOptions options)
            {
                options.API = contextAPI switch
                {
                    ContextAPI.OpenGL => new GraphicsAPI(contextAPI, ContextProfile.Core, ContextFlags.Debug, _PreferredOGLVersion),
                    ContextAPI.Vulkan => new GraphicsAPI(contextAPI, ContextProfile.Core, ContextFlags.Debug, _PreferredVulkanVersion),
                    _ => throw new NotImplementedException()
                };

                IWindow window = Silk.NET.Windowing.Window.Create(options);
                window.Resize += OnWindowResized;
                window.FocusChanged += OnWindowFocusedChanged;
                window.Closing += OnWindowClosing;

                return window;
            }

            try
            {
                _Window = ConstructWindowImpl(windowOptions);
                _Window.Initialize();
            }
            catch (GlfwException glfwException) when (glfwException.ErrorCode is ErrorCode.VersionUnavailable)
            {
                throw new OpenGLVersionNotSupportedException(_PreferredOGLVersion);
            }

            InputManager.Instance.RegisterView(Window);

            DetermineVSyncRefreshRate();
        }

        private void DetermineVSyncRefreshRate()
        {
            const double default_refresh_rate = 60d;

            double refreshRate;

            if (Window.Monitor.VideoMode.RefreshRate.HasValue)
            {
                refreshRate = Window.Monitor.VideoMode.RefreshRate.Value;
            }
            else
            {
                refreshRate = default_refresh_rate;
                Log.Error("No monitor detected VSync framerate will be set to default value.");
            }

            _MinimumFrameTime = TimeSpan.FromSeconds(1d / refreshRate);
            Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AutomataWindow), $"VSync framerate configured to {refreshRate} FPS."));
        }

        #endregion Creation


        #region Runtime

        public async Task RunAsync()
        {
            try
            {
                Stopwatch deltaTimer = new Stopwatch();
                BoundedConcurrentQueue<double> fps = new BoundedConcurrentQueue<double>(60);
                fps.Enqueue(0d); // so we don't get a 'Sequence contains no elements' exception.

                while (!Window.IsClosing)
                {
                    TimeSpan deltaTime = deltaTimer.Elapsed;

                    deltaTimer.Restart();
                    Window.DoEvents();

                    if (InputManager.Instance.IsKeyPressed(Key.Escape))
                    {
                        Window.Close();
                    }

                    if (!Window.IsClosing)
                    {
                        await World.GlobalUpdateAsync(deltaTime);
                        InputManager.Instance.CheckAndExecuteInputActions();

                        Window.DoEvents();
                        Window.SwapBuffers();
                    }

                    if (CheckWaitForNextMonitorRefresh())
                    {
                        WaitForNextMonitorRefresh(deltaTimer);
                    }

                    fps.Enqueue(1d / deltaTimer.Elapsed.TotalSeconds);
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

        private bool CheckWaitForNextMonitorRefresh() => Window.VSync is VSyncMode.On;

        private void WaitForNextMonitorRefresh(Stopwatch deltaTimer) { SpinWait.SpinUntil(() => deltaTimer.Elapsed >= _MinimumFrameTime); }

        #endregion Runtime


        #region Events

        public event WindowResizedEventHandler? Resized;

        public event WindowFocusChangedEventHandler? FocusChanged;

        public event WindowClosingEventHandler? Closing;

        private void OnWindowResized(Size size)
        {
            GLAPI.Instance.GL.Viewport(Window.Size);
            Resized?.Invoke(this, (Vector2i)Size);
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
