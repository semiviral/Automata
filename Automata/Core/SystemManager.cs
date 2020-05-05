#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Automata.Core
{
    public static class SystemManager
    {
        public const int NATIVE_SYSTEM_ORDER = int.MinValue;
        public const int INPUT_SYSTEM_ORDER = -10000;
        public const int DEFAULT_SYSTEM_ORDER = 0;
        public const int FINAL_SYSTEM_ORDER = int.MinValue;

        private static readonly SortedList<int, List<ComponentSystem>> _systems;
        private static readonly Dictionary<Type, ComponentSystem> _systemsByType;

        static SystemManager()
        {
            _systems = new SortedList<int, List<ComponentSystem>>();
            _systemsByType = new Dictionary<Type, ComponentSystem>();
        }

        public static void GlobalUpdate()
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach ((int _, List<ComponentSystem> systems) in _systems)
            {
                foreach (ComponentSystem system in systems)
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
        }

        public static void RegisterSystem<T>(int order = DEFAULT_SYSTEM_ORDER) where T : ComponentSystem
        {
            Type typeT = typeof(T);

            if (_systemsByType.ContainsKey(typeT))
            {
                throw new Exception("System type already instantiated.");
            }

            if (!_systems.ContainsKey(order))
            {
                _systems.Add(order, new List<ComponentSystem>());
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

            _systems[order].Add(componentSystem);
            _systemsByType.Add(typeT, componentSystem);

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
