#region

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
using Silk.NET.GLFW;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;
using ErrorCode = Silk.NET.GLFW.ErrorCode;

#endregion


namespace Automata.Engine.Rendering.GLFW
{
    public delegate void WindowResizedEventHandler(object sender, Vector2i newSize);

    public delegate void WindowFocusChangedEventHandler(object sender, bool isFocused);

    public delegate void WindowClosingEventHandler(object sender);

    public class AutomataWindow : Singleton<AutomataWindow>
    {
        private readonly APIVersion _FallbackOGLVersion = new APIVersion(3, 3);
        private readonly APIVersion _PreferredOGLVersion = new APIVersion(4, 3);

        private TimeSpan _MinimumFrameTime;

        private IWindow? _Window;

        private IWindow Window
        {
            get
            {
                if (_Window is null) throw new InvalidOperationException("Window has not been created.");
                else return _Window;
            }
        }

        public Vector2i Size { get => (Vector2i)Window.Size; set => Window.Size = (Size)value; }

        public Vector2i Position { get => (Vector2i)Window.Position; set => Window.Position = (Point)value; }

        public Vector2i Center => Size / 2;

        public string Title { get => Window.Title; set => Window.Title = value; }

        public bool Focused { get; private set; }

        public AutomataWindow() => Focused = true;

        public event WindowResizedEventHandler? Resized;
        public event WindowFocusChangedEventHandler? FocusChanged;
        public event WindowClosingEventHandler? Closing;

        public void CreateWindow(WindowOptions windowOptions)
        {
            IWindow ConstructWindow(WindowOptions options, bool useOGLFallbackVersion)
            {
                options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible | ContextFlags.Debug,
                    useOGLFallbackVersion ? _FallbackOGLVersion : _PreferredOGLVersion);

                IWindow window = Silk.NET.Windowing.Window.Create(options);
                window.Resize += OnWindowResized;
                window.FocusChanged += OnWindowFocusedChanged;
                window.Closing += OnWindowClosing;

                return window;
            }

            try
            {
                _Window = ConstructWindow(windowOptions, false);
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

            if (Window.Monitor.VideoMode.RefreshRate.HasValue) refreshRate = Window.Monitor.VideoMode.RefreshRate.Value;
            else
            {
                refreshRate = default_refresh_rate;
                Log.Error("No monitor detected VSync framerate will be set to default value.");
            }

            _MinimumFrameTime = TimeSpan.FromSeconds(1d / refreshRate);
            Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AutomataWindow), $"VSync framerate configured to {refreshRate} FPS."));
        }

        public void Run()
        {
            try
            {
                Stopwatch deltaTimer = new Stopwatch();
                TimeSpan deltaTime = TimeSpan.Zero;
                BoundedConcurrentQueue<double> fps = new BoundedConcurrentQueue<double>(60);

                while (!Window.IsClosing)
                {
                    deltaTimer.Restart();
                    Window.DoEvents();

                    if (InputManager.Instance.IsKeyPressed(Key.Escape)) Window.Close();

                    if (!Window.IsClosing) World.GlobalUpdate(deltaTime);

                    Window.DoEvents();
                    Window.SwapBuffers();

                    if (CheckWaitForNextMonitorRefresh()) WaitForNextMonitorRefresh(deltaTimer);

                    deltaTime = deltaTimer.Elapsed;
                    fps.Enqueue(1d / deltaTime.TotalSeconds);
                    Title = $"Automata {fps.Average():0.00} FPS";
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(_LogFormat, $"exception occured: {ex}\r\n{ex.StackTrace}"));
            }
        }

        private bool CheckWaitForNextMonitorRefresh() => Window.VSync is VSyncMode.On;
        private void WaitForNextMonitorRefresh(Stopwatch deltaTimer) => Thread.Sleep(Math.Max((_MinimumFrameTime - deltaTimer.Elapsed).Milliseconds, 0));

        public GL GetGL() => GL.GetApi(Window.GLContext);

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

        private void OnWindowClosing() => Closing?.Invoke(this);
    }
}
