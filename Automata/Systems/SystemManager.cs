#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automata.Collections;
using Automata.Components;
using Automata.Entities;
using Serilog;

// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace Automata.Systems
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
        private readonly OrderedList<ComponentSystem> _ComponentSystems;

        public SystemManager()
        {
            _ComponentSystems = new OrderedList<ComponentSystem>();
            _ComponentSystems.AddLast(new FirstOrderSystem());
            _ComponentSystems.AddLast(new DefaultOrderSystem());
            _ComponentSystems.AddLast(new RenderOrderSystem());
            _ComponentSystems.AddLast(new LastOrderSystem());
        }

        public void Update(EntityManager entityManager, Stopwatch frameTimer)
        {
            foreach (ComponentSystem componentSystem in _ComponentSystems.Where(componentSystem =>
                componentSystem.Enabled && VerifyHandledTypesExist(entityManager, componentSystem)))
            {
                componentSystem.Update(entityManager, frameTimer.Elapsed);
            }

            foreach (IComponentChangeable changeable in entityManager.GetComponents<IComponentChangeable>())
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

            foreach (Type type in componentSystem.HandledComponents?.Types ?? Enumerable.Empty<Type>())
            {
                if (!typeof(IComponent).IsAssignableFrom(type))
                {
                    throw new TypeLoadException($"Type '{type}' does not inherit '{nameof(IComponent)}'.");
                }
            }

            switch (order)
            {
                case SystemRegistrationOrder.Before:
                    _ComponentSystems.AddBefore<TUpdateAround>(componentSystem);
                    break;
                case SystemRegistrationOrder.After:
                    _ComponentSystems.AddAfter<TUpdateAround>(componentSystem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }

            componentSystem.Registered();

            Log.Debug($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
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

        private static bool VerifyHandledTypesExist(EntityManager entityManager, ComponentSystem componentSystem) =>
            componentSystem.HandledComponents is null
            || (componentSystem.HandledComponents.Types.Count == 0)
            || componentSystem.HandledComponents.Types.Any(type => entityManager.GetComponentCount(type) > 0);

        #endregion

        #region IDisposable

        private bool _Disposed;

        private void DisposeInternal()
        {
            foreach (ComponentSystem componentSystem in _ComponentSystems)
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
