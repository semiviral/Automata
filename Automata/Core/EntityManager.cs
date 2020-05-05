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
        private static readonly Type IComponentType = typeof(IComponent);

        public static int MainThreadID { get; }

        private static Dictionary<Guid, IEntity> _Entities { get; }
        private static Dictionary<Type, List<IEntity>> _EntitiesByComponent { get; }

        static EntityManager()
        {
            MainThreadID = Thread.CurrentThread.ManagedThreadId;

            _Entities = new Dictionary<Guid, IEntity>();
            _EntitiesByComponent = new Dictionary<Type, List<IEntity>>();
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

            _Entities.Add(entity.ID, entity);

            Log.Verbose($"{nameof(EntityManager)} registered new entity '{entity.ID}' (#{_Entities.Count}).");
        }

        public static void RegisterComponent<T>(IEntity entity) where T : IComponent
        {
            if (entity.TryAddComponent<T>())
            {
                Type typeT = typeof(T);
                if (!_EntitiesByComponent.ContainsKey(typeT))
                {
                    _EntitiesByComponent.Add(typeof(T), new List<IEntity>());
                }

                _EntitiesByComponent[typeT].Add(entity);
            }
        }

        public static void RegisterComponent<T>(Guid entityID) where T : IComponent
        {
            IEntity entity = _Entities[entityID];

            if (entity.TryAddComponent<T>())
            {
                Type typeT = typeof(T);
                if (!_EntitiesByComponent.ContainsKey(typeT))
                {
                    _EntitiesByComponent.Add(typeof(T), new List<IEntity>());
                }

                _EntitiesByComponent[typeT].Add(entity);
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

            return _EntitiesByComponent[typeT];
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
                    foreach (IEntity entity in _EntitiesByComponent[componentType])
                    {
                        matchingEntityIDs.Add(entity.ID);
                    }

                    continue;
                }

                List<Guid> matchedEntities = new List<Guid>();

                foreach (IEntity entity in _EntitiesByComponent[componentType])
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
                yield return _Entities[entityID];
            }
        }

        public static IEnumerable<T> GetComponents<T>() where T : IComponent
        {
            Type typeT = typeof(T);

            if (!_EntitiesByComponent.ContainsKey(typeT))
            {
                return Enumerable.Empty<T>();
            }

            return _EntitiesByComponent[typeT].Select(entity => entity.GetComponent<T>());
        }

        #endregion
    }
}
