#region

using System.Collections.Generic;

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

        static SystemManager() => _systems = new SortedList<int, List<ComponentSystem>>();

        public static void GlobalUpdate()
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach ((int _, List<ComponentSystem> systems) in _systems)
            {
                foreach (ComponentSystem system in systems)
                {
                    system.Update();
                }
            }
        }

        public static void RegisterSystem(ComponentSystem componentSystem, int order = DEFAULT_SYSTEM_ORDER)
        {
            if (!_systems.ContainsKey(order))
            {
                _systems.Add(order, new List<ComponentSystem>());
            }

            _systems[order].Add(componentSystem);
        }
    }
}
