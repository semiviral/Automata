#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Automata.Engine.Entities;
using Automata.Engine.Systems;
using Serilog;

#endregion


namespace Automata.Engine
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
            SystemManager = new SystemManager(this);
            EntityManager = new EntityManager();

            Active = active;
        }

        public static void RegisterWorld(string name, World world)
        {
            if (Worlds.ContainsKey(name)) throw new ArgumentException(name);

            Worlds.Add(name, world);

            Log.Information($"({nameof(World)}) Registered {nameof(World)}: '{name}' {world.GetType()}");
        }

        public static bool TryGetWorld(string name, [NotNullWhen(true)] out World? world) => Worlds.TryGetValue(name, out world);

        public static async ValueTask GlobalUpdate(TimeSpan deltaTime)
        {
            foreach ((string _, World world) in Worlds.Where(world => world.Value.Active)) await world.Update(deltaTime);
        }

        protected virtual async ValueTask Update(TimeSpan deltaTime) => await SystemManager.Update(EntityManager, deltaTime);


        #region IDisposable

        private bool _Disposed;

        protected virtual void DisposeInternal() { SystemManager.Dispose(); }

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
