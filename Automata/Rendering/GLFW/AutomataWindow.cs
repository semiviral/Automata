#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using Automata.Input;
using Automata.Numerics;
using Automata.Worlds;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.Input.Common;
using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Rendering.GLFW
{
    public class AutomataWindow : Singleton<AutomataWindow>
    {
        #region Private Member Variables

        private readonly Stopwatch _DeltaTimer;

        private IWindow? _Window;
        private TimeSpan _MinimumFrameTime;
        private bool _RegisteredInput;

        private IWindow Window
        {
            get
            {
                if (_Window == null)
                {
                    throw new NullReferenceException(nameof(Window));
                }
                else
                {
                    return _Window;
                }
            }
            set =>
                throw new InvalidOperationException(
                    $"Property '{nameof(Window)}' cannot be set. Use '{nameof(CreateWindow)}' or one of its overloads instead."
                );
        }

        #endregion


        #region Public Member Variables

        public IGLContext? GLContext => Window.GLContext;

        public IVkSurface Surface
        {
            get
            {
                Debug.Assert(Window != null);

                if (Window.VkSurface == null)
                {
                    throw new NotSupportedException("Vulkan is not supported by windowing.");
                }

                return Window.VkSurface;
            }
        }

        public Vector2i Size { get; private set; }
        public bool Focused { get; private set; }

        public Vector2i Position => new Vector2i(Window.Position.X, Window.Position.Y);

        #endregion

        #region Events

        public event WindowResizedEventHandler? Resized;
        public event WindowFocusChangedEventHandler? FocusChanged;
        public event WindowClosingEventHandler? Closing;

        #endregion

        public AutomataWindow()
        {
            AssignSingletonInstance(this);

            _DeltaTimer = new Stopwatch();
        }

        public void CreateWindow(WindowOptions windowOptions)
        {
            _Window = Silk.NET.Windowing.Window.Create(windowOptions);
            _Window.Resize += OnWindowResized;
            _Window.FocusChanged += OnWindowFocusedChanged;
            _Window.Closing += OnWindowClosing;

            Resized += (sender, newSize) => Size = newSize;
            FocusChanged += (sender, isFocused) => Focused = isFocused;

            Size = new Vector2i(Window.Size.Width, Window.Size.Height);
            Focused = true;
        }

        public void Initialize()
        {
            Window.Initialize();

            RegisterInput();

            _MinimumFrameTime = Window.Monitor?.VideoMode.RefreshRate != null
                ? TimeSpan.FromSeconds(1d / Window.Monitor.VideoMode.RefreshRate.Value)
                : TimeSpan.FromSeconds(1d / 60d);
        }

        public void RegisterInput()
        {
            if (_RegisteredInput)
            {
                return;
            }

            InputManager.Instance.RegisterView(Window);
        }

        public void Run()
        {
            try
            {
                while (!Window.IsClosing)
                {
                    TimeSpan delta = _DeltaTimer.Elapsed;
                    _DeltaTimer.Restart();

                    Window.DoEvents();

                    if (InputManager.Instance.IsKeyPressed(Key.Escape))
                    {
                        Window.Close();
                    }

                    if (!Window.IsClosing)
                    {
                        World.GlobalUpdate(delta);
                    }

                    Window.DoEvents();
                    Window.SwapBuffers();

#if true
                    Window.Title = $"Automata ({1d / delta.TotalSeconds:00})";
#endif

                    if (CheckWaitForNextMonitorRefresh())
                    {
                        WaitForNextMonitorRefresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(_LogFormat, $"exception occured: {ex}"));
                throw;
            }
        }

        private bool CheckWaitForNextMonitorRefresh()
        {
            switch (Window.VSync)
            {
                case VSyncMode.On:
                    return true;
                case VSyncMode.Off:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WaitForNextMonitorRefresh()
        {
            TimeSpan frameWait = _MinimumFrameTime - _DeltaTimer.Elapsed;
            Thread.Sleep(frameWait <= TimeSpan.Zero ? TimeSpan.Zero : frameWait);
        }


        #region Event Subscriptors

        private void OnWindowResized(Size newSize) => Resized?.Invoke(this, Unsafe.As<Size, Vector2i>(ref newSize));

        private void OnWindowFocusedChanged(bool focused) => FocusChanged?.Invoke(this, Focused);

        private void OnWindowClosing() => Closing?.Invoke(this);

        #endregion
    }
}
