#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Worlds;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.Input.Common;
using Silk.NET.Windowing.Common;

#endregion


namespace Automata.Engine.Rendering.GLFW
{
    public class AutomataWindow : Singleton<AutomataWindow>
    {
        private readonly Stopwatch _DeltaTimer;

        private IWindow? _Window;
        private TimeSpan _MinimumFrameTime;

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

        public bool Focused { get; private set; }

        public event WindowResizedEventHandler? Resized;
        public event WindowFocusChangedEventHandler? FocusChanged;
        public event WindowClosingEventHandler? Closing;

        public AutomataWindow()
        {
            AssignSingletonInstance(this);

            _DeltaTimer = new Stopwatch();

            Focused = true;
        }

        public void CreateWindow(WindowOptions windowOptions)
        {
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
                while (!Window.IsClosing)
                {
                    _DeltaTimer.Restart();

                    Window.DoEvents();

                    if (InputManager.Instance.IsKeyPressed(Key.Escape)) Window.Close();

                    if (!Window.IsClosing) World.GlobalUpdate(_DeltaTimer);

                    Window.DoEvents();
                    Window.SwapBuffers();

                    if (CheckWaitForNextMonitorRefresh()) WaitForNextMonitorRefresh();
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(_LogFormat, $"exception occured: {ex}"));
                throw;
            }
        }

        private bool CheckWaitForNextMonitorRefresh() =>
            Window.VSync switch
            {
                VSyncMode.On => true,
                VSyncMode.Off => false,
                _ => throw new ArgumentOutOfRangeException()
            };

        private void WaitForNextMonitorRefresh()
        {
            TimeSpan frameWait = _MinimumFrameTime - _DeltaTimer.Elapsed;
            Thread.Sleep(frameWait <= TimeSpan.Zero ? TimeSpan.Zero : frameWait);
        }


        #region Event Subscriptors

        private void OnWindowResized(Size size) => Resized?.Invoke(this, Size);

        private void OnWindowFocusedChanged(bool focused)
        {
            Focused = focused;
            FocusChanged?.Invoke(this, focused);
        }

        private void OnWindowClosing() => Closing?.Invoke(this);

        #endregion
    }
}
