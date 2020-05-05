#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Automata.Core
{
    public static class SystemManager
    {
        public const int INITIAL_SYSTEM_ORDER = int.MinValue;
        public const int INPUT_SYSTEM_ORDER = -100000;
        public const int DEFAULT_SYSTEM_ORDER = 0;
        public const int MESH_COMPOSITION_SYSTEM_ORDER = 99000;
        public const int RENDER_SYSTEM_ORDER = 100000;
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
