#region

using System;
using System.Diagnostics;
using System.Threading;
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
        private readonly Stopwatch _DeltaTimer;

        private IWindow? _Window;
        private TimeSpan _MinimumFrameTime;


        public IWindow Window
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

        public Vector2i Position => new Vector2i(Window.Position.X, Window.Position.Y);
        public Vector2i Size => new Vector2i(Window.Size.Width, Window.Size.Height);

        public AutomataWindow()
        {
            AssignSingletonInstance(this);

            _DeltaTimer = new Stopwatch();
        }

        public void CreateWindow(WindowOptions windowOptions)
        {
            _Window = Silk.NET.Windowing.Window.Create(windowOptions);

            _MinimumFrameTime = Window.Monitor?.VideoMode.RefreshRate != null
                ? TimeSpan.FromSeconds(1d / _Window.Monitor.VideoMode.RefreshRate.Value)
                : TimeSpan.FromSeconds(1d / 72d);
        }

        public void Run()
        {
            try
            {
                while (!Window.IsClosing)
                {
                    if (Input.Input.Instance.IsKeyPressed(Key.Escape))
                    {
                        Window.Close();
                    }

                    TimeSpan delta = _DeltaTimer.Elapsed;
                    _DeltaTimer.Restart();

#if DEBUG

                    Window.Title = $"Automata ({1d / delta.TotalSeconds:00})";

#endif

                    Window.DoEvents();

                    if (!Window.IsClosing)
                    {
                        World.GlobalUpdate(delta);
                    }

                    TimeSpan frameWait = _MinimumFrameTime - _DeltaTimer.Elapsed;
                    Thread.Sleep(frameWait <= TimeSpan.Zero ? TimeSpan.Zero : frameWait);
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(_LogFormat, $"exception occured: {ex}"));
                throw;
            }
        }
    }
}
