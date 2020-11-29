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

        public TransformMatrixSystem(World world) : base(world)
        {
            AutomataWindow.Instance.Resized += GameWindowResized;
            GameWindowResized(null, AutomataWindow.Instance.Size);
        }

        public override ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            foreach ((Entity entity, Transform transform) in entityManager.GetEntitiesWithComponents<Transform>().Where(tuple => tuple.Component1.Changed))
            {
                if (entity.TryComponent(out Camera? camera))
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

                transform.Changed = false;
            }

            // for now I'm entertaining that its POSSIBLE a Camera might not have a transform
            // so we process the projections separately
            if (_UpdateProjections)
            {
                foreach (Camera camera in entityManager.GetComponents<Camera>())
                {
                    camera.Projection = IProjection.CreateFromProjector(camera.Projector);
                }
            }

            _UpdateProjections = false;
            return ValueTask.CompletedTask;
        }


        #region Events

        private void GameWindowResized(object? sender, Vector2<int> size) => _UpdateProjections = true;

        #endregion
    }
}
