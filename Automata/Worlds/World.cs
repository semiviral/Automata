#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automata.Components;
using Automata.Entities;
using Automata.Systems;
using Serilog;

#endregion

namespace Automata.Worlds
{
    public class World : IDisposable
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
        }

        public static void RegisterWorld(string name, World world)
        {
            if (Worlds.ContainsKey(name))
            {
                throw new ArgumentException(name);
            }

            Worlds.Add(name, world);

            Log.Debug($"({nameof(World)}) Registered {nameof(World)}: '{name}' {world.GetType()}");
        }

        public static void TryGetWorld(string name, out World? world) => Worlds.TryGetValue(name, out world);

        public static void GlobalUpdate(Stopwatch frameTimer)
        {
            foreach ((string _, World world) in Worlds.Where(kvp => kvp.Value.Active))
            {
                Debug.Assert(world.Active, "World must be active to update.");

                world.Update(frameTimer);
            }
        }

        protected virtual void Update(Stopwatch frameTimer)
        {
            // update system manager for frame
            SystemManager.Update(EntityManager, frameTimer);

            foreach (IComponentChangeable changeable in EntityManager.GetComponents<IComponentChangeable>())
            {
                changeable.Changed = false;
            }
        }

        #region IDisposable

        private bool _Disposed;

        protected virtual void DisposeInternal()
        {
            SystemManager.Dispose();
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
