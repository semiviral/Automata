#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Automata.GLFW;
using Automata.Worlds;
using Serilog;

#endregion

namespace Automata.Rendering
{
    public class CameraMatrixSystem : ComponentSystem
    {
        private bool _HasGameWindowResized;
        private float _NewAspectRatio;

        public CameraMatrixSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(Translation),
                typeof(Rotation)
            };

            GameWindow.Validate();

            Debug.Assert(GameWindow.Instance != null);
            Debug.Assert(GameWindow.Instance.Window != null);

            GameWindowResized(GameWindow.Instance.Window.Size);
            GameWindow.Instance.Window.Resize += GameWindowResized;
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<Camera>())
            {
                Camera camera = entity.GetComponent<Camera>();

                // adjust projection
                if (_HasGameWindowResized)
                {
                    camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(AutomataMath.ToRadians(90f), _NewAspectRatio, 0.1f, 1000f);
                }

                // adjust view
                if (entity.TryGetComponent(out Translation translation)
                    && entity.TryGetComponent(out Rotation rotation)
                    && (translation.Changed || rotation.Changed))
                {
                    camera.View = AutomataMath.MatrixFromTranslationAndRotationWithScaleToView(1f, translation, rotation);
                }
            }

            _HasGameWindowResized = false;
        }

        private void GameWindowResized(Size size)
        {
            _HasGameWindowResized = true;
            _NewAspectRatio = (float)size.Width / size.Height;
        }
    }
}
