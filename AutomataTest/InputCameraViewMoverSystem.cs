#region

using System.Numerics;
using Automata.Core;
using Automata.Rendering;

#endregion

namespace AutomataTest
{
    public class InputCameraViewMoverSystem : ComponentSystem
    {
        public InputCameraViewMoverSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(RenderedShader),
                typeof(Translation),
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Camera _, RenderedShader renderedShader, Translation translation, Rotation rotation) in entityManager
                .GetComponents<Camera, RenderedShader, Translation, Rotation>())
            {
                if (!translation.Changed && !rotation.Changed)
                {
                    continue;
                }

                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(translation.Value);
                Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation.Value);
                Matrix4x4 finalMatrix = Matrix4x4.Multiply(translationMatrix, rotationMatrix);

                //renderedShader.Shader.SetUniform("view", Matrix4x4.CreateWorld(translation.Normal, forward, up));
                renderedShader.Shader.SetUniform("view", finalMatrix);
            }
        }
    }
}
