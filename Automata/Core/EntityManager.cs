#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Automata.Exceptions;
using Serilog;

#endregion

namespace Automata.Core
{
    public static class EntityManager
    {
        public static int MainThreadID { get; }

        private static Dictionary<Guid, IEntity> Entities { get; }
        private static Dictionary<Type, List<IEntity>> EntitiesByComponent { get; }
        private static Dictionary<Type, int> ComponentCountByType { get; }

        static EntityManager()
        {
            MainThreadID = Thread.CurrentThread.ManagedThreadId;

            Entities = new Dictionary<Guid, IEntity>();
            EntitiesByComponent = new Dictionary<Type, List<IEntity>>();
            ComponentCountByType = new Dictionary<Type, int>();
        }

        #region Register .. Data

        /// <summary>
        ///     Registers given entity with <see cref="EntityManager" />.
        /// </summary>
        /// <param name="entity">Entity to register.</param>
        /// <exception cref="AsynchronousAccessException">Thrown when method is called from outside main thread.</exception>
        /// <exception cref="NullReferenceException">Thrown when given <see cref="IEntity" /> is null.</exception>
        public static void RegisterEntity(IEntity entity)
        {
            if (Thread.CurrentThread.ManagedThreadId != MainThreadID)
            {
                throw new AsynchronousAccessException(ExceptionFormats.EntityManagerAsynchronousAccessException);
            }

            if (entity == null)
            {
                throw new NullReferenceException(nameof(entity));
            }

            Entities.Add(entity.ID, entity);

            Log.Verbose($"{nameof(EntityManager)} registered new {nameof(IEntity)} '{entity.ID}' (#{Entities.Count}).");
        }

        /// <summary>
        ///     Registers the specified component instance to the given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to add component to.</param>
        /// <param name="component">Component to add.</param>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public static void RegisterComponent(IEntity entity, IComponent component) =>
            RegisterComponentInternal(entity, component.GetType(), component);

        /// <summary>
        ///     Adds the specified component to the given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to add component to.</param>
        /// <typeparam name="T">Type of component to add.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public static void RegisterComponent<T>(IEntity entity) where T : IComponent =>
            RegisterComponentInternal(entity, typeof(T), null);

        private static void RegisterComponentInternal(IEntity entity, Type type, IComponent? component)
        {
            if (!EntitiesByComponent.ContainsKey(type))
            {
                EntitiesByComponent.Add(type, new List<IEntity>());
            }

            if (!ComponentCountByType.ContainsKey(type))
            {
                ComponentCountByType.Add(type, 0);
            }

            if (entity.TryAddComponent(component ?? (IComponent)Activator.CreateInstance(type)))
            {
                EntitiesByComponent[type].Add(entity);
                ComponentCountByType[type] += 1;
            }
        }

        #endregion

        #region Remove .. Data

        /// <summary>
        ///     Removes the specified component instance from given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to remove component from.</param>
        /// <typeparam name="T">Type of component to remove.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public static void RemoveComponent<T>(IEntity entity) where T : IComponent
        {
            Type typeT = typeof(T);

            if (entity.TryRemoveComponent<T>())
            {
                EntitiesByComponent[typeT].Remove(entity);
            }
        }

        #endregion

        #region Get .. Data

        /// <summary>
        ///     Returns <see cref="IEnumerable{T}" /> of entities containing component <see cref="T" />.
        /// </summary>
        /// <typeparam name="T"><see cref="IComponent" /> type to retrieve by.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of entities containing component <see cref="T" />.</returns>
        /// <exception />
        public static IEnumerable<IEntity> GetEntitiesWithComponent<T>() where T : IComponent
        {
            Type typeT = typeof(T);

            return !EntitiesByComponent.ContainsKey(typeT) ? Enumerable.Empty<IEntity>() : EntitiesByComponent[typeT];
        }

        // todo this should accept only component types
        public static IEnumerable<IEntity> GetEntitiesWithComponents(params Type[] componentTypes)
        {
            bool iteratedAny = false;
            HashSet<Guid> matchingEntityIDs = new HashSet<Guid>();

            foreach (Type componentType in componentTypes)
            {
                if (!typeof(IComponent).IsAssignableFrom(componentType))
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

        /// <summary>
        ///     Returns all instances of components of type <see cref="T" />
        /// </summary>
        /// <typeparam name="T">Type of <see cref="IComponent" /> to return.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of all instances of <see cref="IComponent" /> type <see cref="T" />.</returns>
        public static IEnumerable<T> GetComponents<T>() where T : IComponent
        {
            Type typeT = typeof(T);

            if (!EntitiesByComponent.ContainsKey(typeT))
            {
                return Enumerable.Empty<T>();
            }

            return EntitiesByComponent[typeT].Select(entity => entity.GetComponent<T>());
        }

        public static int GetComponentCount<T>() where T : IComponent => ComponentCountByType[typeof(T)];

        public static int GetComponentCount(Type type)
        {
            if (!typeof(IComponent).IsAssignableFrom(type))
            {
                throw new TypeLoadException(type.ToString());
            }

            if (ComponentCountByType.TryGetValue(type, out int count))
            {
                return count;
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }
}
