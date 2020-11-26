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

        /// <summary>
        ///     Returns instantiated system of type <see cref="TSystem" />, if any.
        /// </summary>
        /// <typeparam name="TSystem"><see cref="ComponentSystem" /> <see cref="Type" /> to return instance of.</typeparam>
        /// <returns>Instantiated <see cref="ComponentSystem" /> of type <see cref="TSystem" />, if any.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     <see cref="ComponentSystem" /> of given type <see cref="TSystem" /> has not been instantiated.
        /// </exception>
        public TSystem GetSystem<TSystem>() where TSystem : ComponentSystem => (_ComponentSystems[typeof(TSystem)] as TSystem)!;


        #region Update

        public async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan deltaTime)
        {
            foreach (ComponentSystem componentSystem in _ComponentSystems)
            {
                if (componentSystem.Enabled && CheckSystemHandledTypesExist(entityManager, componentSystem))
                {
                    await componentSystem.UpdateAsync(entityManager, deltaTime).ConfigureAwait(false);
                }
            }
        }

        private bool CheckSystemHandledTypesExist(EntityManager entityManager, ComponentSystem componentSystem)
        {
            static bool HandledComponentsExistImpl(EntityManager entityManager, HandledComponents handledComponents) => handledComponents.Strategy switch
            {
                EnumerationStrategy.Any when handledComponents.All(type => entityManager.GetComponentCount(type) == 0u) => true,
                EnumerationStrategy.All when handledComponents.Any(type => entityManager.GetComponentCount(type) > 0u) => true,
                EnumerationStrategy.None when handledComponents.All(type => entityManager.GetComponentCount(type) > 0u) => true,
                _ => false
            };

            if (_HandledComponentsArrays.TryGetValue(componentSystem.GetType(), out HandledComponents[]? handleComponentsArray))
            {
                return handleComponentsArray!.Length is 0
                       || handleComponentsArray.Any(handledComponents => HandledComponentsExistImpl(entityManager, handledComponents));
            }
            else
            {
                return true;
            }
        }

        #endregion


        #region RegisterSystem

        /// <summary>
        ///     Registers a new system of type <see cref="TSystem" />.
        /// </summary>
        /// <typeparam name="TSystem"><see cref="ComponentSystem" /> type to instantiate.</typeparam>
        /// <typeparam name="TBefore"><see cref="ComponentSystem" /> type to update system after.</typeparam>
        /// <exception cref="Exception">
        ///     Thrown when system type <see cref="TSystem" /> has already been instantiated.
        /// </exception>
        /// <exception cref="TypeLoadException">
        ///     Thrown when system of type <see cref="TBefore" /> doesn't exist.
        /// </exception>
        public void RegisterBefore<TSystem, TBefore>()
            where TSystem : ComponentSystem, new()
            where TBefore : ComponentSystem
        {
            TSystem componentSystem = new TSystem();
            _ComponentSystems.AddBefore<TBefore>(componentSystem);
            RegisterSystemInternal(componentSystem);
        }

        public void RegisterAfter<TSystem, TAfter>()
            where TSystem : ComponentSystem, new()
            where TAfter : ComponentSystem
        {
            TSystem componentSystem = new TSystem();
            _ComponentSystems.AddAfter<TAfter>(componentSystem);
            RegisterSystemInternal(componentSystem);
        }

        public void RegisterFirst<TSystem>() where TSystem : ComponentSystem, new()
        {
            TSystem componentSystem = new TSystem();
            _ComponentSystems.AddFirst(componentSystem);
            RegisterSystemInternal(componentSystem);
        }

        public void RegisterLast<TSystem>() where TSystem : ComponentSystem, new()
        {
            TSystem componentSystem = new TSystem();
            _ComponentSystems.AddLast(componentSystem);
            RegisterSystemInternal(componentSystem);
        }

        private void RegisterSystemInternal<TSystem>(TSystem componentSystem) where TSystem : ComponentSystem, new()
        {
            if (TryGetHandledComponents<TSystem>(out IEnumerable<HandledComponents>? handledComponentsEnumerable))
            {
                _HandledComponentsArrays.Add(typeof(TSystem), handledComponentsEnumerable.ToArray());
            }

            componentSystem.SetCurrentWorld(_CurrentWorld);
            componentSystem.Registered(_CurrentWorld.EntityManager);

            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        private static bool TryGetHandledComponents<TSystem>([NotNullWhen(true)] out IEnumerable<HandledComponents>? handledComponentsEnumerable)
            where TSystem : ComponentSystem, new()
        {
            MethodBase? methodBase = typeof(TSystem).GetMethod(nameof(ComponentSystem.UpdateAsync));
            handledComponentsEnumerable = methodBase?.GetCustomAttributes<HandledComponents>();

            return handledComponentsEnumerable is not null;
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
