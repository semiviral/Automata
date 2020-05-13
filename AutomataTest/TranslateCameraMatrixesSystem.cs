#region

using System.Numerics;
using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Rendering;

#endregion

namespace AutomataTest
{
    public class TranslateCameraMatrixesSystem : ComponentSystem
    {
        public TranslateCameraMatrixesSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(Translation),
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Camera camera, Translation translation, Rotation rotation) in entityManager.GetComponents<Camera, Translation, Rotation>())
            {
                if (!translation.Changed && !rotation.Changed)
                {
                    continue;
                }

                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(translation.Value);
                Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation.Value);
                Matrix4x4 finalViewMatrix = Matrix4x4.Multiply(translationMatrix, rotationMatrix);

                camera.View = finalViewMatrix;
            }
        }
    }
}
