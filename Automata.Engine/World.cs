using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Automata.Engine
{
    public class World : IDisposable
    {
        #region Static

        private static Dictionary<string, World> Worlds { get; }

        public static void RegisterWorld(string name, World world)
        {
            if (Worlds.ContainsKey(name)) throw new ArgumentException(name);

            Worlds.Add(name, world);

            Log.Information($"({nameof(World)}) Registered {nameof(World)}: '{name}' {world.GetType()}");
        }

        public static bool TryGetWorld(string name, [NotNullWhen(true)] out World? world) => Worlds.TryGetValue(name, out world);

        public static async ValueTask GlobalUpdate(TimeSpan deltaTime)
        {
            foreach (World world in Worlds.Values.Where(world => world.Active)) await world.Update(deltaTime);
        }

        public static void DisposeWorlds()
        {
            foreach ((_, World world) in Worlds) world.Dispose();
        }

        #endregion


        public EntityManager EntityManager { get; }
        public SystemManager SystemManager { get; }
        public bool Active { get; set; }

        static World() => Worlds = new Dictionary<string, World>();

        protected World(bool active)
        {
            SystemManager = new SystemManager(this);
            EntityManager = new EntityManager();

            Active = active;
        }

        protected virtual async ValueTask Update(TimeSpan deltaTime) => await SystemManager.Update(EntityManager, deltaTime);


        #region IDisposable

        private bool _Disposed;

        protected virtual void DisposeInternal()
        {
            EntityManager.Dispose();
            SystemManager.Dispose();
        }

        public void Dispose()
        {
            if (_Disposed) return;

            DisposeInternal();
            GC.SuppressFinalize(this);
            _Disposed = true;
        }

        #endregion
    }
}
