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

                if ((entity.TryGetComponent(out Scale? scale) && scale.Changed)
                    | (entity.TryGetComponent(out Translation? translation) && translation.Changed)
                    | (entity.TryGetComponent(out Rotation? rotation) && rotation.Changed))
                {
                    camera.View = Matrix4x4.Identity;
                    camera.View *= Matrix4x4.CreateScale(scale?.Value ?? Scale.DEFAULT);
                    camera.View *= Matrix4x4.CreateTranslation(translation?.Value ?? Vector3.Zero);
                    camera.View *= Matrix4x4.CreateFromQuaternion(rotation?.Value ?? Quaternion.Identity);
                }

                // adjust projection
                if (_NewAspectRatio > 0f)
                {
                    camera.CalculateProjection(_NewAspectRatio);
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
