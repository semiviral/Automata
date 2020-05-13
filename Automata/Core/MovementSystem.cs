#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Singletons;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Core
{
    public class MovementSystem : ComponentSystem
    {
        public MovementSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Translation)
            };

            Input.Validate();
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Translation translation, InputListener _) in entityManager.GetComponents<Translation, InputListener>())
            {
                if (Input.Instance.IsKeyPressed(Key.W))
                {
                    translation.Value += Vector3.UnitZ * deltaTime;
                }

                if (Input.Instance.IsKeyPressed(Key.S))
                {
                    translation.Value -= Vector3.UnitZ * deltaTime;
                }

                if (Input.Instance.IsKeyPressed(Key.D))
                {
                    translation.Value += Vector3.UnitX * deltaTime;
                }

                if (Input.Instance.IsKeyPressed(Key.A))
                {
                    translation.Value -= Vector3.UnitX * deltaTime;
                }
            }
        }
    }
}
