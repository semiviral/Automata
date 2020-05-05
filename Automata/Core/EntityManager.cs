#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog;

#endregion

namespace Automata.Core
{
    public static class EntityManager
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Type IComponentType = typeof(IComponent);

        public static int MainThreadID { get; }

        private static Dictionary<Guid, IEntity> Entities { get; }
        private static Dictionary<Type, List<IEntity>> EntitiesByComponent { get; }

        static EntityManager()
        {
            MainThreadID = Thread.CurrentThread.ManagedThreadId;

            Entities = new Dictionary<Guid, IEntity>();
            EntitiesByComponent = new Dictionary<Type, List<IEntity>>();
        }

        #region Register .. Data

        public static void RegisterEntity(IEntity entity)
        {
            if (Thread.CurrentThread.ManagedThreadId != MainThreadID)
            {
                throw new Exception("Cannot modify entity collection asynchronously.");
            }

            if (entity == null)
            {
                throw new NullReferenceException(nameof(entity));
            }

            Entities.Add(entity.ID, entity);

            Log.Verbose($"{nameof(EntityManager)} registered new entity '{entity.ID}' (#{Entities.Count}).");
        }

        public static void RegisterComponent(IEntity entity, IComponent component) => entity.AddComponent(component);

        public static void RegisterComponent<T>(IEntity entity) where T : IComponent
        {
            if (entity.TryAddComponent<T>())
            {
                Type typeT = typeof(T);
                if (!EntitiesByComponent.ContainsKey(typeT))
                {
                    EntitiesByComponent.Add(typeof(T), new List<IEntity>());
                }

                EntitiesByComponent[typeT].Add(entity);
            }
        }

        public static void RegisterComponent<T>(Guid entityID) where T : IComponent
        {
            IEntity entity = Entities[entityID];

            if (entity.TryAddComponent<T>())
            {
                Type typeT = typeof(T);
                if (!EntitiesByComponent.ContainsKey(typeT))
                {
                    EntitiesByComponent.Add(typeof(T), new List<IEntity>());
                }

                EntitiesByComponent[typeT].Add(entity);
            }
        }

        #endregion


        #region Get .. Data

        public static IEnumerable<IEntity> GetEntitiesWithComponent<T>() where T : IComponent
        {
            Type typeT = typeof(T);

            if (!IComponentType.IsAssignableFrom(typeT))
            {
                throw new TypeLoadException(typeT.ToString());
            }

            return EntitiesByComponent[typeT];
        }

        // todo this should accept only component types
        public static IEnumerable<IEntity> GetEntitiesWithComponents(params Type[] componentTypes)
        {
            bool iteratedAny = false;
            HashSet<Guid> matchingEntityIDs = new HashSet<Guid>();

            foreach (Type componentType in componentTypes)
            {
                if (!IComponentType.IsAssignableFrom(componentType))
                {
                    throw new TypeLoadException(componentType.ToString());
                }
                else if (!iteratedAny) // first iteration
                {
                    foreach (IEntity entity in EntitiesByComponent[componentType])
                    {
                        matchingEntityIDs.Add(entity.ID);
                    }

                    continue;
                }

                List<Guid> matchedEntities = new List<Guid>();

                foreach (IEntity entity in EntitiesByComponent[componentType])
                {
                    if (matchedEntities.Contains(entity.ID))
                    {
                        matchedEntities.Add(entity.ID);
                    }
                }

                matchingEntityIDs = new HashSet<Guid>(matchedEntities);

                iteratedAny = true;
            }

            foreach (Guid entityID in matchingEntityIDs)
            {
                yield return Entities[entityID];
            }
        }

        public static IEnumerable<T> GetComponents<T>() where T : IComponent
        {
            Type typeT = typeof(T);

            if (!EntitiesByComponent.ContainsKey(typeT))
            {
                return Enumerable.Empty<T>();
            }

            return EntitiesByComponent[typeT].Select(entity => entity.GetComponent<T>());
        }

        #endregion
    }
}
