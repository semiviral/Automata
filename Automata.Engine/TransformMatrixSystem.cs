using System;
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
            foreach ((Entity entity, Camera camera) in entityManager.GetEntitiesWithComponents<Camera>())
            {
                // check for changes and update current camera's view matrix
                if ((entity.TryFind(out Scale? cameraScale) && cameraScale.Changed)
                    | (entity.TryFind(out Rotation? cameraRotation) && cameraRotation.Changed)
                    | (entity.TryFind(out Translation? cameraTranslation) && cameraTranslation.Changed))
                {
                    Matrix4x4 view = Matrix4x4.Identity;

                    if (cameraScale is not null)
                    {
                        view *= Matrix4x4.CreateScale(cameraScale.Value);
                    }

                    if (cameraRotation is not null)
                    {
                        view *= Matrix4x4.CreateFromQuaternion(cameraRotation.Value);
                    }

                    if (cameraTranslation is not null)
                    {
                        view *= Matrix4x4.CreateTranslation(cameraTranslation.Value);
                    }

                    // if we've calculated a new view matrix, invert it and apply to camera
                    if (Matrix4x4.Invert(view, out Matrix4x4 inverted))
                    {
                        view = inverted;
                    }

                    camera.View = view;
                }

                if (_UpdateProjections)
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

            foreach ((Entity objectEntity, RenderModel renderModel) in entityManager.GetEntitiesWithComponents<RenderModel>())
            {
                if (((objectEntity.TryFind(out Translation? modelTranslation) && modelTranslation.Changed)
                     | (objectEntity.TryFind(out Rotation? modelRotation) && modelRotation.Changed)
                     | (objectEntity.TryFind(out Scale? modelScale) && modelScale.Changed))
                    || renderModel.Changed)
                {
                    renderModel.Model = Matrix4x4.Identity;

                    if (modelTranslation is not null)
                    {
                        renderModel.Model *= Matrix4x4.CreateTranslation(modelTranslation.Value);
                    }

                    if (modelRotation is not null)
                    {
                        renderModel.Model *= Matrix4x4.CreateFromQuaternion(modelRotation.Value);
                    }

                    if (modelScale is not null)
                    {
                        renderModel.Model *= Matrix4x4.CreateScale(modelScale.Value);
                    }
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
