using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering;

namespace Automata.Engine
{
    public class TransformMatrixSystem : ComponentSystem
    {
        private bool _UpdateProjections;

        public TransformMatrixSystem()
        {
            AutomataWindow.Instance.Resized += GameWindowResized;
            GameWindowResized(null, AutomataWindow.Instance.Size);
        }

        public override ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            foreach ((Entity entity, Transform transform) in entityManager.GetEntitiesWithComponents<Transform>().Where(tuple => tuple.Component1.Changed))
            {
                if (entity.TryFind(out Camera? camera))
                {
                    Matrix4x4 matrix = Matrix4x4.Identity;
                    matrix *= Matrix4x4.CreateScale(transform.Scale);
                    matrix *= Matrix4x4.CreateFromQuaternion(transform.Rotation);
                    matrix *= Matrix4x4.CreateTranslation(transform.Translation);
                    transform.Matrix = matrix;

                    if (Matrix4x4.Invert(matrix, out Matrix4x4 view))
                    {
                        camera.View = view;
                    }
                }
                else
                {
                    Matrix4x4 matrix = Matrix4x4.Identity;
                    matrix *= Matrix4x4.CreateTranslation(transform.Translation);
                    matrix *= Matrix4x4.CreateFromQuaternion(transform.Rotation);
                    matrix *= Matrix4x4.CreateScale(transform.Scale);
                    transform.Matrix = matrix;
                }
            }

            if (_UpdateProjections)
            {
                foreach (Camera camera in entityManager.GetComponents<Camera>())
                {
                    camera.Projection = camera.Projector switch
                    {
                        // todo handle near, far, clipping planes and FOV in the projection itself
                        Projector.Perspective => new PerspectiveProjection(90f, AutomataWindow.Instance.AspectRatio, 0.1f, 1000f),
                        Projector.Orthographic => new OrthographicProjection(AutomataWindow.Instance.Size, 0.1f, 1000f),
                        _ => camera.Projection
                    };
                }
            }

            _UpdateProjections = false;
            return ValueTask.CompletedTask;
        }


        #region Events

        private void GameWindowResized(object? sender, Vector2i newSize) => _UpdateProjections = true;

        #endregion
    }
}
