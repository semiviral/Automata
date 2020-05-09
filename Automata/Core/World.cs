#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;

#endregion

namespace Automata.Core
{
    public class World
    {
        private static Dictionary<string, World> Worlds { get; }

        static World() => Worlds = new Dictionary<string, World>();

        public static void RegisterWorld(string name, World world)
        {
            if (Worlds.ContainsKey(name))
            {
                throw new ArgumentException(name);
            }
            else if (world == null)
            {
                throw new NullReferenceException(nameof(world));
            }

            Worlds.Add(name, world);

            Log.Verbose($"Registered new {nameof(World)}: \"{name}\" of type '{world.GetType()}'");
        }

        public static void TryGetWorld(string name, out World? world) => Worlds.TryGetValue(name, out world);

        public static void GlobalUpdate()
        {
            foreach ((string _, World world) in Worlds)
            {
                if (!world.Active)
                {
                    continue;
                }

                world.Update();
            }
        }

        private readonly Stopwatch _DeltaTimer;

        private double _LastDeltaTime;
        private bool _LastDeltaTimeChanged;

        public EntityManager EntityManager { get; }
        public SystemManager SystemManager { get; }
        public bool Active { get; set; }

        public double LastDeltaTime
        {
            get => _LastDeltaTime;
            set
            {
                _LastDeltaTime = value;
                _LastDeltaTimeChanged = true;
            }
        }

        protected World(bool active)
        {
            _DeltaTimer = new Stopwatch();

            EntityManager = new EntityManager();
            SystemManager = new SystemManager();

            Active = active;
        }

        protected virtual void Update()
        {
            // calculate delta time
            LastDeltaTime = _DeltaTimer.Elapsed.TotalSeconds;

            // reset delta timer
            _DeltaTimer.Restart();

            // update system manager for frame
            SystemManager.Update(EntityManager, (float)LastDeltaTime);
        }
    }
}
