#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Automata.Core;
using Automata.Core.Systems;
using Automata.Singletons;

#endregion

namespace Automata.Rendering
{
    /// <summary>
    ///     Pre-rendering system applies changed view matrices to their respective shaders.
    /// </summary>
    public class PreRenderSystem : ComponentSystem
    {
        private bool _HasGameWindowResized;
        private Vector2 _ResizedSize;

        public PreRenderSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(RenderedShader)
            };

            if (!GameWindow.Validate())
            {
                throw new InvalidOperationException();
            }

            Debug.Assert(GameWindow.Instance != null);
            Debug.Assert(GameWindow.Instance.Window != null);

            GameWindowResized(GameWindow.Instance.Window.Size);
            GameWindow.Instance.Window.Resize += GameWindowResized;
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (Camera camera in entityManager.GetComponents<Camera>())
            {
                if (_HasGameWindowResized)
                {
                    camera.Projection = Matrix4x4.CreatePerspective(AutomataMath.ToRadians(90f), _ResizedSize.X / _ResizedSize.Y, 0.1f, 100f);
                }
                else if (!camera.Changed)
                {
                    continue;
                }

                foreach (RenderedShader renderedShader in entityManager.GetComponents<RenderedShader>())
                {
                    renderedShader.Shader.SetUniform(nameof(camera.View), camera.View);
                    renderedShader.Shader.SetUniform(nameof(camera.Projection), camera.Projection);
                    renderedShader.Shader.SetUniform(nameof(camera.Model), camera.Model);
                }
            }

            _HasGameWindowResized = false;
        }

        private void GameWindowResized(Size size)
        {
            _HasGameWindowResized = true;
            _ResizedSize = new Vector2(size.Width, size.Height);
        }
    }
}
