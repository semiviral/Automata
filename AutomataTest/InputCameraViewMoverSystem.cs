using System;
using System.Numerics;
using Automata.Core;
using Automata.Input;
using Automata.Rendering;
using Serilog;
using Silk.NET.Input.Common;

namespace AutomataTest
{
    public class InputCameraViewMoverSystem : ComponentSystem
    {
        public InputCameraViewMoverSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(CameraEntityComponent),
                typeof(RenderedShaderComponent),
                typeof(KeyboardInput)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (IEntity entity in entityManager
                .GetEntitiesWithComponents<CameraEntityComponent, RenderedShaderComponent, KeyboardInput, Translation>())
            {
                RenderedShaderComponent renderedShaderComponent = entity.GetComponent<RenderedShaderComponent>();
                KeyboardInput keyboardInput = entity.GetComponent<KeyboardInput>();
                Translation inputDirectionVectorComponent = entity.GetComponent<Translation>();

                Vector3 modificationVector = Vector3.Zero;

                if (keyboardInput.KeysDown.Contains(Key.D))
                {
                    modificationVector.X = 1f;
                }

                if (keyboardInput.KeysDown.Contains(Key.A))
                {
                    modificationVector.X = -1f;
                }

                if (modificationVector == Vector3.Zero)
                {
                    return;
                }

                inputDirectionVectorComponent.Position += (modificationVector * deltaTime *  10f);

                const float radius = 3f;
                //renderedShaderComponent.Shader.SetUniform("view", Matrix4x4.CreateLookAt(, Vector3.Zero, Vector3.UnitY));
            }
        }
    }
}
