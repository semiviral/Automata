#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automata.Entity;
using Automata.System;
using Serilog;

#endregion

namespace Automata.Worlds
{
    public class World
    {
        private static Dictionary<string, World> Worlds { get; }

        public EntityManager EntityManager { get; }
        public SystemManager SystemManager { get; }
        public bool Active { get; set; }

        static World() => Worlds = new Dictionary<string, World>();

        protected World(bool active)
        {
            EntityManager = new EntityManager();
            SystemManager = new SystemManager();

            Active = active;

            RegisterDefaultSystems();
        }

        public static void RegisterWorld(string name, World world)
        {
            if (Worlds.ContainsKey(name))
            {
                throw new ArgumentException(name);
            }

            Worlds.Add(name, world);

            Log.Information($"Registered new {nameof(World)}: \"{name}\" of type '{world.GetType()}'");
        }

        public static void TryGetWorld(string name, out World? world) => Worlds.TryGetValue(name, out world);

        public static void GlobalUpdate(TimeSpan delta)
        {
            foreach ((string _, World world) in Worlds.Where(kvp => kvp.Value.Active))
            {
                Debug.Assert(world.Active, "World must be active to update.");

                world.Update(delta);
            }
        }

        private void RegisterDefaultSystems()
        {
            SystemManager.RegisterSystem<InternalEntityChangedResetSystem, LastOrderSystem>(SystemRegistrationOrder.After);
        }

        protected virtual void Update(TimeSpan delta)
        {
            // update system manager for frame
            SystemManager.Update(EntityManager, delta);

            foreach (IComponentChangeable changeable in EntityManager.GetComponents<IComponentChangeable>())
            {
                changeable.Changed = false;
            }
        }
    }
}
