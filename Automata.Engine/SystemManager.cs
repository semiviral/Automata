using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Automata.Engine.Collections;
using Serilog;

namespace Automata.Engine
{
    public class FirstOrderSystem : ComponentSystem { }

    public class DefaultOrderSystem : ComponentSystem { }

    public class LastOrderSystem : ComponentSystem { }

    public sealed class SystemManager : IDisposable
    {
        private readonly IOrderedCollection<ComponentSystem> _ComponentSystems;
        private readonly World _CurrentWorld;
        private readonly Dictionary<Type, HandledComponents[]> _HandledComponentsArrays;

        public SystemManager(World currentWorld)
        {
            _ComponentSystems = new OrderedList<ComponentSystem>();
            _HandledComponentsArrays = new Dictionary<Type, HandledComponents[]>();
            _CurrentWorld = currentWorld;

            RegisterLast<FirstOrderSystem>();
            RegisterLast<DefaultOrderSystem>();
            RegisterLast<LastOrderSystem>();
        }

        public async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan deltaTime)
        {
            foreach (ComponentSystem componentSystem in _ComponentSystems)
            {
                if (componentSystem.Enabled && VerifyHandledComponentsExistForSystem(entityManager, componentSystem))
                {
                    await componentSystem.UpdateAsync(entityManager, deltaTime).ConfigureAwait(false);
                }
            }

            foreach (ComponentChangeable changeable in entityManager.GetComponentsExplicit<ComponentChangeable>())
            {
                changeable.Changed = false;
            }
        }

        /// <summary>
        ///     Returns instantiated system of type <see cref="TSystem" />, if any.
        /// </summary>
        /// <typeparam name="TSystem"><see cref="ComponentSystem" /> <see cref="Type" /> to return instance of.</typeparam>
        /// <returns>Instantiated <see cref="ComponentSystem" /> of type <see cref="TSystem" />, if any.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     <see cref="ComponentSystem" /> of given type <see cref="TSystem" /> has not been instantiated.
        /// </exception>
        public TSystem GetSystem<TSystem>() where TSystem : ComponentSystem => (_ComponentSystems[typeof(TSystem)] as TSystem)!;


        #region Helper Methods

        private bool VerifyHandledComponentsExistForSystem(EntityManager entityManager, ComponentSystem componentSystem)
        {
            if (_HandledComponentsArrays.TryGetValue(componentSystem.GetType(), out HandledComponents[]? handleComponentsArray))
            {
                if (handleComponentsArray!.Length is 0)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            foreach (HandledComponents handledComponents in handleComponentsArray)
            {
                switch (handledComponents.Strategy)
                {
                    case EnumerationStrategy.None when handledComponents.All(type => entityManager.GetComponentCount(type) == 0u):
                    case EnumerationStrategy.Any when handledComponents.Any(type => entityManager.GetComponentCount(type) > 0u):
                    case EnumerationStrategy.All when handledComponents.All(type => entityManager.GetComponentCount(type) > 0u): return true;
                    default: continue;
                }
            }

            throw new InvalidOperationException($"{nameof(SystemManager)} has encountered an invalid state attempting to enumerate handled component types.");
        }

        #endregion


        #region RegisterSystem

        /// <summary>
        ///     Registers a new system of type <see cref="TSystem" />.
        /// </summary>
        /// <typeparam name="TSystem"><see cref="ComponentSystem" /> type to instantiate.</typeparam>
        /// <typeparam name="TBeforeSystem"><see cref="ComponentSystem" /> type to update system after.</typeparam>
        /// <exception cref="Exception">
        ///     Thrown when system type <see cref="TSystem" /> has already been instantiated.
        /// </exception>
        /// <exception cref="TypeLoadException">
        ///     Thrown when system of type <see cref="TBeforeSystem" /> doesn't exist.
        /// </exception>
        public void RegisterBefore<TSystem, TBeforeSystem>()
            where TSystem : ComponentSystem, new()
            where TBeforeSystem : ComponentSystem
        {
            TSystem componentSystem = new();
            _ComponentSystems.AddBefore<TBeforeSystem>(componentSystem);
            RegisterSystemInternal(componentSystem);

            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        public void RegisterAfter<TSystem, TAfterSystem>()
            where TSystem : ComponentSystem, new()
            where TAfterSystem : ComponentSystem
        {
            TSystem componentSystem = new();
            _ComponentSystems.AddBefore<TAfterSystem>(componentSystem);
            RegisterSystemInternal(componentSystem);

            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        public void RegisterFirst<TSystem>() where TSystem : ComponentSystem, new()
        {
            TSystem componentSystem = new();
            _ComponentSystems.AddFirst(componentSystem);
            RegisterSystemInternal(componentSystem);

            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        public void RegisterLast<TSystem>() where TSystem : ComponentSystem, new()
        {
            TSystem componentSystem = new();
            _ComponentSystems.AddLast(componentSystem);
            RegisterSystemInternal(componentSystem);

            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        private void RegisterSystemInternal<TSystem>(TSystem componentSystem) where TSystem : ComponentSystem, new()
        {
            if (TryGetHandledComponents<TSystem>(out IEnumerable<HandledComponents>? handledComponentsEnumerable))
            {
                _HandledComponentsArrays.Add(typeof(TSystem), handledComponentsEnumerable.ToArray());
            }

            componentSystem.SetCurrentWorld(_CurrentWorld);
            componentSystem.Registered(_CurrentWorld.EntityManager);
        }

        private static bool TryGetHandledComponents<TSystem>([NotNullWhen(true)] out IEnumerable<HandledComponents>? handledComponentsEnumerable)
            where TSystem : ComponentSystem, new()
        {
            MethodBase? methodBase = typeof(TSystem).GetMethod(nameof(ComponentSystem.UpdateAsync));
            handledComponentsEnumerable = methodBase?.GetCustomAttributes<HandledComponents>();

            return methodBase is not null;
        }

        #endregion


        #region IDisposable

        private bool _Disposed;

        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            foreach (ComponentSystem componentSystem in _ComponentSystems)
            {
                if (componentSystem is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _Disposed = true;
        }

        #endregion
    }
}
