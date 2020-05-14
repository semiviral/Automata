#region

using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Singletons;

#endregion

namespace Automata.Rendering
{
    public class CameraMatrixesSystem : ComponentSystem
    {
        private bool _HasGameWindowResized;
        private float _NewFOV;

        public CameraMatrixesSystem()
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

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<Camera>())
            {
                Camera camera = entity.GetComponent<Camera>();

                // adjust perspective
                if (_HasGameWindowResized)
                {
                    camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(AutomataMath.ToRadians(90f), _NewFOV, 0.1f, 1000f);
                }

                // adjust view
                if (entity.TryGetComponent(out Translation translation)
                    && entity.TryGetComponent(out Rotation rotation)
                    && (translation.Changed || rotation.Changed))
                {
                    camera.View = AutomataMath.MatrixFromTranslationAndRotationWithScaleToView(1f, rotation, translation);
                }
            }

            _HasGameWindowResized = false;
        }

        private void GameWindowResized(Size size)
        {
            _HasGameWindowResized = true;
            _NewFOV = (float)size.Width / size.Height;
        }
    }
}
