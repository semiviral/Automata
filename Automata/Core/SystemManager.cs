#region

using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

#endregion

namespace Automata.Core
{
    public static class SystemManager
    {
        /// <summary>
        ///     The very first order updated.
        /// </summary>
        /// <remarks>
        ///     This is also the order that any IView.DoUpdate() will be called.
        /// </remarks>
        public const int INITIAL_SYSTEM_ORDER = int.MinValue;

        /// <summary>
        ///     Order for updating input systems.
        /// </summary>
        public const int INPUT_SYSTEM_ORDER = -100000;

        /// <summary>
        ///     Default system order.
        /// </summary>
        /// <remarks>
        ///     If you're unsure what system order to use for a non-critical system, this is the safest one.
        /// </remarks>
        public const int DEFAULT_SYSTEM_ORDER = 0;

        /// <summary>
        ///     Order used for composing meshes.
        /// </summary>
        public const int MESH_COMPOSITION_SYSTEM_ORDER = 99000;

        /// <summary>
        ///     Order used for rendering operations.
        /// </summary>
        /// <remarks>
        ///     This is also the order that any IView.DoRender() will be called.
        /// </remarks>
        public const int RENDER_SYSTEM_ORDER = 100000;

        /// <summary>
        ///     Final order that will be executed on each frame.
        /// </summary>
        public const int FINAL_SYSTEM_ORDER = int.MaxValue;

        private static readonly SortedList<int, ComponentSystem> _systems;
        private static readonly Dictionary<Type, ComponentSystem> _systemsByType;

        static SystemManager()
        {
            _systems = new SortedList<int, ComponentSystem>();
            _systemsByType = new Dictionary<Type, ComponentSystem>();
        }

        public static void Update()
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach ((int _, ComponentSystem system) in _systems)
            {
                if (!system.IsEnabled)
                {
                    system.Enabled();
                    system.IsEnabled = true;
                }

                if (system.UtilizedComponentTypes.Any(type => EntityManager.GetComponentCount(type) <= 0))
                {
                    continue;
                }

                system.Update();
            }
        }

        /// <summary>
        ///     Registers a new system of type <see cref="T" />.
        /// </summary>
        /// <param name="order">Order to update system.</param>
        /// <typeparam name="T"><see cref="ComponentSystem" /> <see cref="Type" /> to instantiate.</typeparam>
        /// <exception cref="Exception">Thrown when system type <see cref="T" /> has already been instantiated.</exception>
        /// <exception cref="TypeLoadException">
        ///     Thrown when the <see cref="SystemManager" /> runs out of keys that exceed the given <see cref="order" /> value
        ///     passed.
        /// </exception>
        /// <remarks>
        ///     For reference on what ordering to use, there are several constants within the <see cref="SystemManager" /> class to
        ///     act as default values for certain types of systems.
        /// </remarks>
        public static void RegisterSystem<T>(int order = DEFAULT_SYSTEM_ORDER) where T : ComponentSystem
        {
            Type componentSystemTypeT = typeof(T);

            if (_systemsByType.ContainsKey(componentSystemTypeT))
            {
                throw new Exception("System type already instantiated.");
            }

            int finalOrder = order;

            while (_systems.ContainsKey(finalOrder))
            {
                finalOrder += 1;

                if (finalOrder == int.MaxValue)
                {
                    throw new Exception($"{nameof(SystemManager)} has run out of keys to assign after order {order}.");
                }
            }

            T componentSystem = Activator.CreateInstance<T>();

            foreach (Type type in componentSystem.UtilizedComponentTypes)
            {
                if (!typeof(IComponent).IsAssignableFrom(type))
                {
                    throw new TypeLoadException(
                        $"A given type in '{nameof(componentSystem.UtilizedComponentTypes)}' does not inherit '{nameof(IComponent)}' ({componentSystem.GetType()}: {type}).");
                }
            }

            _systems.Add(finalOrder, componentSystem);
            _systemsByType.Add(componentSystemTypeT, componentSystem);

            componentSystem.Registered();
        }

        /// <summary>
        ///     Returns instantiated system of type <see cref="T" />, if any.
        /// </summary>
        /// <typeparam name="T"><see cref="ComponentSystem" /> <see cref="Type" /> to return instance of.</typeparam>
        /// <returns>Instantiated <see cref="ComponentSystem" /> of type <see cref="T" />, if any.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     <see cref="ComponentSystem" /> of given type <see cref="T" /> has not been instantiated.
        /// </exception>
        public static T GetSystem<T>() where T : ComponentSystem
        {
            Type typeT = typeof(T);

            if (!_systemsByType.ContainsKey(typeT))
            {
                throw new KeyNotFoundException("System type has not been instantiated.");
            }

            return (T)_systemsByType[typeT];
        }
    }
}
