#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Systems;

#endregion

namespace Automata.Engine.Rendering
{
    public class CameraMatrixSystem : ComponentSystem
    {
        private float _NewAspectRatio;

        public CameraMatrixSystem()
        {
            GameWindowResized(null!, AutomataWindow.Instance.Size);
            AutomataWindow.Instance.Resized += GameWindowResized;
        }

        [HandlesComponents(DistinctionStrategy.All, typeof(Camera))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<Camera>())
            {
                Camera camera = entity.GetComponent<Camera>();
                Matrix4x4 calculatedView = Matrix4x4.Identity;
                bool recalculateView = false;

                if (entity.TryGetComponent(out Scale? scale))
                {
                    calculatedView *= Matrix4x4.CreateScale(scale.Value);
                    recalculateView |= scale.Changed;
                }

                if (entity.TryGetComponent(out Translation? translation))
                {
                    calculatedView *= Matrix4x4.CreateTranslation(translation.Value);
                    recalculateView |= translation.Changed;
                }

                if (entity.TryGetComponent(out Rotation? rotation))
                {
                    calculatedView *= Matrix4x4.CreateFromQuaternion(rotation.Value);
                    recalculateView |= rotation.Changed;
                }

                if (recalculateView)
                {
                    camera.View = calculatedView;
                }

                // adjust projection
                if (_NewAspectRatio > 0f)
                {
                    const float near_clipping_plane = 0.1f;
                    const float far_clipping_plane = 1000f;

                    camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(AutomataMath.ToRadians(90f), _NewAspectRatio, near_clipping_plane,
                        far_clipping_plane);
                    camera.ProjectionParameters = new Vector4(1f, near_clipping_plane, far_clipping_plane, 1f / far_clipping_plane);
                }
            }

            _NewAspectRatio = 0f;
        }

        private void GameWindowResized(object sender, Vector2i newSize)
        {
            _NewAspectRatio = (float)newSize.X / (float)newSize.Y;
        }
    }
}
