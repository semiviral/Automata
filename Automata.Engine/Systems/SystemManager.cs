#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Serilog;

// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace Automata.Engine.Systems
{
    public enum SystemRegistrationOrder : byte
    {
        Before,
        After
    }

    public class FirstOrderSystem : ComponentSystem { }

    public class DefaultOrderSystem : ComponentSystem { }

    public class RenderOrderSystem : ComponentSystem { }

    public class LastOrderSystem : ComponentSystem { }

    public sealed class SystemManager : IDisposable
    {
        private readonly OrderedList<(ComponentTypes HandledTypes, ComponentSystem ComponentSystem)> _ComponentSystems;

        public SystemManager()
        {
            _ComponentSystems = new OrderedList<(ComponentTypes, ComponentSystem)>();
            _ComponentSystems.AddLast((ComponentTypes.Empty, new FirstOrderSystem()));
            _ComponentSystems.AddLast((ComponentTypes.Empty, new DefaultOrderSystem()));
            _ComponentSystems.AddLast((ComponentTypes.Empty, new RenderOrderSystem()));
            _ComponentSystems.AddLast((ComponentTypes.Empty, new LastOrderSystem()));
        }

        public void Update(EntityManager entityManager, Stopwatch frameTimer)
        {
            foreach ((ComponentTypes handledTypes, ComponentSystem componentSystem) in _ComponentSystems)
            {
                if (!componentSystem.Enabled || !VerifyHandledTypesExist(entityManager, handledTypes))
                {
                    continue;
                }

                componentSystem.Update(entityManager, frameTimer.Elapsed);
            }

            foreach (IComponentChangeable changeable in entityManager.GetComponentsAssignableFrom<IComponentChangeable>())
            {
                changeable.Changed = false;
            }
        }

        /// <summary>
        ///     Registers a new system of type <see cref="TSystem" />.
        /// </summary>
        /// <typeparam name="TSystem"><see cref="ComponentSystem" /> type to instantiate.</typeparam>
        /// <typeparam name="TUpdateAround"><see cref="ComponentSystem" /> type to update system after.</typeparam>
        /// <exception cref="Exception">
        ///     Thrown when system type <see cref="TSystem" /> has already been instantiated.
        /// </exception>
        /// <exception cref="TypeLoadException">
        ///     Thrown when system of type <see cref="TUpdateAround" /> doesn't exist.
        /// </exception>
        public void RegisterSystem<TSystem, TUpdateAround>(SystemRegistrationOrder order)
            where TSystem : ComponentSystem, new()
            where TUpdateAround : ComponentSystem
        {
            TSystem componentSystem = new TSystem();
            ComponentTypes handledTypes = GetHandledTypesFromComponentSystem(componentSystem);

            switch (order)
            {
                case SystemRegistrationOrder.Before:
                    _ComponentSystems.AddBefore<TUpdateAround>((handledTypes, componentSystem));
                    break;
                case SystemRegistrationOrder.After:
                    _ComponentSystems.AddAfter<TUpdateAround>((handledTypes, componentSystem));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }

            componentSystem.Registered();

            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        private ComponentTypes GetHandledTypesFromComponentSystem(ComponentSystem componentSystem)
        {
            MethodBase? updateMethodBase = componentSystem.GetType().GetMethod(nameof(componentSystem.Update));

            if (updateMethodBase is null)
            {
                return ComponentTypes.Empty;
            }

            HandlesComponents? handlesComponents = updateMethodBase.GetCustomAttribute<HandlesComponents>();

            if (handlesComponents is null)
            {
                return ComponentTypes.Empty;
            }

            return handlesComponents.Types;
        }

        /// <summary>
        ///     Returns instantiated system of type <see cref="T" />, if any.
        /// </summary>
        /// <typeparam name="T"><see cref="ComponentSystem" /> <see cref="Type" /> to return instance of.</typeparam>
        /// <returns>Instantiated <see cref="ComponentSystem" /> of type <see cref="T" />, if any.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     <see cref="ComponentSystem" /> of given type <see cref="T" /> has not been instantiated.
        /// </exception>
        public T GetSystem<T>() where T : ComponentSystem => (T)_ComponentSystems[typeof(T)].ComponentSystem;

        #region Helper Methods

        private static bool VerifyHandledTypesExist(EntityManager entityManager, ComponentTypes types) =>
            types.Strategy switch
            {
                DistinctionStrategy.None => types.All(type => entityManager.GetComponentCount(type) == 0),
                DistinctionStrategy.All => types.All(type => entityManager.GetComponentCount(type) > 0),
                DistinctionStrategy.Any => types.Any(type => entityManager.GetComponentCount(type) > 0),
                _ => throw new ArgumentOutOfRangeException()
            };

        #endregion

        #region IDisposable

        private bool _Disposed;

        private void DisposeInternal()
        {
            foreach ((ComponentTypes _, ComponentSystem componentSystem) in _ComponentSystems)
            {
                componentSystem.Dispose();
            }
        }

        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            DisposeInternal();
            GC.SuppressFinalize(this);
            _Disposed = true;
        }

        #endregion
    }
}
