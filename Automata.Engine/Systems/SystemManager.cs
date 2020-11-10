#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Worlds;
using Serilog;

#endregion


namespace Automata.Engine.Systems
{
    public enum SystemRegistrationOrder : byte
    {
        Before,
        After,
        First,
        Last
    }

    public class FirstOrderSystem : ComponentSystem { }

    public class DefaultOrderSystem : ComponentSystem { }

    public class LastOrderSystem : ComponentSystem { }

    public sealed class SystemManager : IDisposable
    {
        private readonly IOrderedCollection<ComponentSystem> _ComponentSystems;
        private readonly Dictionary<Type, ComponentTypes> _HandledTypes;
        private readonly World _CurrentWorld;

        public SystemManager(World currentWorld)
        {
            _ComponentSystems = new OrderedList<ComponentSystem>();
            _HandledTypes = new Dictionary<Type, ComponentTypes>();
            _CurrentWorld = currentWorld;

            RegisterSystem<FirstOrderSystem>(SystemRegistrationOrder.Last);
            RegisterSystem<DefaultOrderSystem>(SystemRegistrationOrder.Last);
            RegisterSystem<LastOrderSystem>(SystemRegistrationOrder.Last);
        }

        public void Update(EntityManager entityManager, TimeSpan deltaTime)
        {
            foreach (ComponentSystem componentSystem in _ComponentSystems)
                if (componentSystem.Enabled || VerifyHandledTypesExist(entityManager, _HandledTypes[componentSystem.GetType()]))
                    componentSystem.Update(entityManager, deltaTime);

            foreach (ComponentChangeable changeable in entityManager.GetComponentsExplicit<ComponentChangeable>()) changeable.Changed = false;
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

            switch (order)
            {
                case SystemRegistrationOrder.Before:
                    _ComponentSystems.AddBefore<TUpdateAround>(componentSystem);
                    break;
                case SystemRegistrationOrder.After:
                    _ComponentSystems.AddAfter<TUpdateAround>(componentSystem);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }

            RegisterHandledTypes<TSystem>();
            componentSystem.SetCurrentWorld(_CurrentWorld);
            componentSystem.Registered(_CurrentWorld.EntityManager);
            Log.Debug($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        private void RegisterSystem<TSystem>(SystemRegistrationOrder order) where TSystem : ComponentSystem, new()
        {
            TSystem componentSystem = new TSystem();

            switch (order)
            {
                case SystemRegistrationOrder.First:
                    _ComponentSystems.AddFirst(componentSystem);
                    break;
                case SystemRegistrationOrder.Last:
                    _ComponentSystems.AddLast(componentSystem);
                    break;
            }

            RegisterHandledTypes<TSystem>();
            componentSystem.Registered(_CurrentWorld.EntityManager);
            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        private void RegisterHandledTypes<TSystem>() where TSystem : ComponentSystem, new() =>
            _HandledTypes.Add(typeof(TSystem), GetHandledTypesFromComponentSystem<TSystem>());

        private static ComponentTypes GetHandledTypesFromComponentSystem<TSystem>() where TSystem : ComponentSystem, new()
        {
            MethodBase? updateMethodBase = typeof(TSystem).GetMethod(nameof(ComponentSystem.Update));
            HandlesComponents? handlesComponents = updateMethodBase?.GetCustomAttribute<HandlesComponents>();
            return handlesComponents is null ? ComponentTypes.Empty : handlesComponents.Types;
        }

        /// <summary>
        ///     Returns instantiated system of type <see cref="T" />, if any.
        /// </summary>
        /// <typeparam name="T"><see cref="ComponentSystem" /> <see cref="Type" /> to return instance of.</typeparam>
        /// <returns>Instantiated <see cref="ComponentSystem" /> of type <see cref="T" />, if any.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     <see cref="ComponentSystem" /> of given type <see cref="T" /> has not been instantiated.
        /// </exception>
        public T GetSystem<T>() where T : ComponentSystem => (T)_ComponentSystems[typeof(T)];


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
            foreach (ComponentSystem componentSystem in _ComponentSystems) componentSystem.Dispose();
        }

        public void Dispose()
        {
            if (_Disposed) return;

            _Disposed = true;

            DisposeInternal();
        }

        #endregion
    }
}
