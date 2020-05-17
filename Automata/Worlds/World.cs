#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Automata.GLFW;
using Serilog;

#endregion

namespace Automata.Worlds
{
    public class World
    {
        private static readonly Stopwatch _DeltaTimer;

        private static readonly TimeSpan _MinimumFrameTime = TimeSpan.FromSeconds(1d / 72d);

        private static Dictionary<string, World> Worlds { get; }

        static World()
        {
            _DeltaTimer = new Stopwatch();

            Worlds = new Dictionary<string, World>();
        }

        public static void RegisterWorld(string name, World world)
        {
            if (world == null)
            {
                throw new NullReferenceException(nameof(world));
            }
            else if (Worlds.ContainsKey(name))
            {
                throw new ArgumentException(name);
            }

            Worlds.Add(name, world);

            Log.Verbose($"Registered new {nameof(World)}: \"{name}\" of type '{world.GetType()}'");
        }

        public static void TryGetWorld(string name, out World? world) => Worlds.TryGetValue(name, out world);

        public static void GlobalUpdate()
        {
            TimeSpan delta = _DeltaTimer.Elapsed;
            _DeltaTimer.Restart();

            foreach ((string _, World world) in Worlds.Where(kvp => kvp.Value.Active))
            {
                Debug.Assert(world.Active, "World must be active to update.");

                world.Update(delta);
            }

            TimeSpan frameWait = _MinimumFrameTime - _DeltaTimer.Elapsed;

            Thread.Sleep(frameWait < TimeSpan.Zero ? TimeSpan.Zero : frameWait);
        }

        public EntityManager EntityManager { get; }
        public SystemManager SystemManager { get; }
        public bool Active { get; set; }

        protected World(bool active)
        {
            EntityManager = new EntityManager();
            SystemManager = new SystemManager();

            Active = active;

            RegisterDefaultSystems();
        }

        private void RegisterDefaultSystems()
        {
            SystemManager.RegisterSystem<ViewDoUpdateSystem, FirstOrderSystem>(SystemRegistrationOrder.After);
            SystemManager.RegisterSystem<ViewDoRenderSystem, LastOrderSystem>(SystemRegistrationOrder.After);
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
