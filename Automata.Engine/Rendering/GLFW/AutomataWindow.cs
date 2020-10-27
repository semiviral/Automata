#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using Automata.Engine.Collections;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Worlds;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.GLFW;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;

#endregion


namespace Automata.Engine.Rendering.GLFW
{
    public class AutomataWindow : Singleton<AutomataWindow>
    {
        private TimeSpan _MinimumFrameTime;

        private IWindow? _Window;

        public IGLContext? GLContext => Window.GLContext;

        public IVkSurface Surface
        {
            get
            {
                if (Window.VkSurface == null) throw new NotSupportedException("Vulkan is not supported by windowing.");

                return Window.VkSurface;
            }
        }

        private IWindow Window
        {
            get
            {
                if (_Window is null) throw new NullReferenceException(nameof(Window));
                else return _Window;
            }
        }

        public Vector2i Size { get => (Vector2i)Window.Size; set => Window.Size = (Size)value; }

        public Vector2i Position { get => (Vector2i)Window.Position; set => Window.Position = (Point)value; }

        public Vector2i Center => Size / 2;

        public string Title { get => Window.Title; set => Window.Title = value; }

        public bool Focused { get; private set; }

        public AutomataWindow()
        {
            AssignSingletonInstance(this);

            Focused = true;
        }

        public event WindowResizedEventHandler? Resized;
        public event WindowFocusChangedEventHandler? FocusChanged;
        public event WindowClosingEventHandler? Closing;

        public void CreateWindow(WindowOptions windowOptions)
        {
            GLFWAPI.Instance.GLFW.WindowHint(WindowHintBool.OpenGLDebugContext, true);

            _Window = Silk.NET.Windowing.Window.Create(windowOptions);
            _Window.Resize += OnWindowResized;
            _Window.FocusChanged += OnWindowFocusedChanged;
            _Window.Closing += OnWindowClosing;

            Window.Initialize();
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
            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AutomataWindow), $"VSync framerate configured to {refreshRate} FPS."));
        }

        public void Run()
        {
            try
            {
                Stopwatch deltaTimer = new Stopwatch();
                BoundedConcurrentQueue<double> fps = new BoundedConcurrentQueue<double>(60);

                while (!Window.IsClosing)
                {
                    deltaTimer.Restart();

                    Window.DoEvents();

                    if (InputManager.Instance.IsKeyPressed(Key.Escape)) Window.Close();

                    if (!Window.IsClosing) World.GlobalUpdate(deltaTimer);

                    Window.DoEvents();
                    Window.SwapBuffers();

                    if (CheckWaitForNextMonitorRefresh()) WaitForNextMonitorRefresh(deltaTimer);

                    fps.Enqueue(1d / deltaTimer.Elapsed.TotalSeconds);
                    Title = $"Automata {fps.Average():0.00} FPS";
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(_LogFormat, $"exception occured: {ex}"));
                throw;
            }
        }

        private bool CheckWaitForNextMonitorRefresh() => Window.VSync == VSyncMode.On;

        private void WaitForNextMonitorRefresh(Stopwatch deltaTimer) => Thread.Sleep(Math.Max((_MinimumFrameTime - deltaTimer.Elapsed).Milliseconds, 0));

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
