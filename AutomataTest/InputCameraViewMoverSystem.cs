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
                if (!translation.Changed)
                {
                    continue;
                }

                renderedShader.Shader.SetUniform("view", Matrix4x4.CreateLookAt(translation.Position,
                    Vector3.Transform(Vector3.UnitZ, rotation.Quaternion),
                    Vector3.Transform(Vector3.UnitY, rotation.Quaternion)));
                //Matrix4x4.CreateLookAt(new Vector3((float)Math.Sin(translation.Position.X * deltaTime), 0f, (float)Math.Cos(translation.Position.Z * deltaTime)), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0)));
            }
        }
    }
}
