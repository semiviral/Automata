#region

using System;
using System.Numerics;
using Automata.Numerics;
using Automata.Rendering.GLFW;
using Automata.Worlds;

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

            GameWindowResized(null!, AutomataWindow.Instance.Size);
            AutomataWindow.Instance.Resized += GameWindowResized;
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<Camera>())
            {
                Camera camera = entity.GetComponent<Camera>();

                // adjust view
                if (entity.TryGetComponent(out Translation translation)
                    && entity.TryGetComponent(out Rotation rotation)
                    && (translation.Changed || rotation.Changed))
                {
                    camera.View = Matrix4x4.Identity
                                  * Matrix4x4.CreateTranslation(translation.Value)
                                  * Matrix4x4.CreateFromQuaternion(rotation.Value);
                }

                // adjust projection
                if (_HasGameWindowResized)
                {
                    const float near_clipping_plane = 0.1f;
                    const float far_clipping_plane = 100f;

                    camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(AutomataMath.ToRadians(90f), _NewAspectRatio, near_clipping_plane,
                        far_clipping_plane);
                    camera.ProjectionParameters = new Vector4(1f, near_clipping_plane, far_clipping_plane, 1f / far_clipping_plane);
                }
            }

            _HasGameWindowResized = false;
        }

        private void GameWindowResized(object sender, Vector2i newSize)
        {
            _HasGameWindowResized = true;
            _NewAspectRatio = (float)newSize.X / (float)newSize.Y;
        }
    }
}
