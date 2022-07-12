using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Automata.Engine.Collections;
using Serilog;

namespace Automata.Engine
{
    public class FirstOrderSystem : ComponentSystem
    {
        public FirstOrderSystem(World world) : base(world) { }
    }

    public class DefaultOrderSystem : ComponentSystem
    {
        public DefaultOrderSystem(World world) : base(world) { }
    }

    public class LastOrderSystem : ComponentSystem
    {
        public LastOrderSystem(World world) : base(world) { }
    }

    public sealed class SystemManager : IDisposable
    {
        private readonly World _World;
        private readonly IOrderedCollection<ComponentSystem> _ComponentSystems;
        private readonly Dictionary<Type, HandledComponents[]> _HandledComponentsArrays;
        private readonly Stopwatch _UpdateStopwatch;

        public SystemManager(World world)
        {
            _World = world;
            _ComponentSystems = new OrderedList<ComponentSystem>();
            _HandledComponentsArrays = new Dictionary<Type, HandledComponents[]>();
            _UpdateStopwatch = new Stopwatch();

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
            foreach (ComponentSystem component_system in _ComponentSystems)
            {
                if (component_system.Enabled && CheckSystemHandledTypesExist(entityManager, component_system))
                {
                    _UpdateStopwatch.Restart();
                    await component_system.UpdateAsync(entityManager, deltaTime).ConfigureAwait(false);
                    _UpdateStopwatch.Stop();

                    if (_UpdateStopwatch.Elapsed >= AutomataWindow.Instance.VSyncFrameTime)
                    {
                        Log.Debug(
                            string.Format(
                                FormatHelper.DEFAULT_LOGGING,
                                nameof(SystemManager),
                                $"Excessive update time ({_UpdateStopwatch.Elapsed.TotalSeconds:0.00}s): {component_system.GetType()}"
                            )
                        );
                    }
                }
            }
        }

        private bool CheckSystemHandledTypesExist(EntityManager entityManager, ComponentSystem componentSystem)
        {
            static bool handled_components_exist_impl_impl(EntityManager entityManager, HandledComponents handledComponents) => handledComponents.Strategy switch
            {
                EnumerationStrategy.Any when handledComponents.All(type => entityManager.GetComponentCount(type) == 0u) => true,
                EnumerationStrategy.All when handledComponents.Any(type => entityManager.GetComponentCount(type) > 0u) => true,
                EnumerationStrategy.None when handledComponents.All(type => entityManager.GetComponentCount(type) > 0u) => true,
                _ => false
            };

            if (_HandledComponentsArrays.TryGetValue(componentSystem.GetType(), out HandledComponents[]? handle_components_array))
            {
                return handle_components_array!.Length is 0
                       || handle_components_array.Any(handledComponents => handled_components_exist_impl_impl(entityManager, handledComponents));
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
            where TSystem : ComponentSystem
            where TBefore : ComponentSystem
        {
            TSystem component_system = CreateSystem<TSystem>();
            _ComponentSystems.AddBefore<TBefore>(component_system);
            RegisterSystemInternal(component_system);
        }

        public void RegisterAfter<TSystem, TAfter>()
            where TSystem : ComponentSystem
            where TAfter : ComponentSystem
        {
            TSystem component_system = CreateSystem<TSystem>();
            _ComponentSystems.AddAfter<TAfter>(component_system);
            RegisterSystemInternal(component_system);
        }

        public void RegisterFirst<TSystem>() where TSystem : ComponentSystem
        {
            TSystem component_system = CreateSystem<TSystem>();
            _ComponentSystems.AddFirst(component_system);
            RegisterSystemInternal(component_system);
        }

        public void RegisterLast<TSystem>() where TSystem : ComponentSystem
        {
            TSystem component_system = CreateSystem<TSystem>();
            _ComponentSystems.AddLast(component_system);
            RegisterSystemInternal(component_system);
        }

        private TSystem CreateSystem<TSystem>() where TSystem : ComponentSystem => (Activator.CreateInstance(typeof(TSystem), _World) as TSystem)!;

        private void RegisterSystemInternal<TSystem>(TSystem componentSystem) where TSystem : ComponentSystem
        {
            if (TryGetHandledComponents<TSystem>(out IEnumerable<HandledComponents>? handled_components_enumerable))
            {
                _HandledComponentsArrays.Add(typeof(TSystem), handled_components_enumerable.ToArray());
            }

            componentSystem.Registered(_World.EntityManager);

            Log.Information($"({nameof(SystemManager)}) Registered {nameof(ComponentSystem)}: {typeof(TSystem)}");
        }

        private static bool TryGetHandledComponents<TSystem>([NotNullWhen(true)] out IEnumerable<HandledComponents>? handledComponentsEnumerable)
            where TSystem : ComponentSystem
        {
            MethodBase? method_base = typeof(TSystem).GetMethod(nameof(ComponentSystem.UpdateAsync));
            handledComponentsEnumerable = method_base?.GetCustomAttributes<HandledComponents>();

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

            foreach (ComponentSystem component_system in _ComponentSystems)
            {
                if (component_system is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _Disposed = true;
        }

        #endregion
    }
}
