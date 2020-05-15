#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automata.Core.Components;
using Automata.Exceptions;
using Serilog;

#endregion

namespace Automata.Core
{
    public class EntityManager
    {
        private Dictionary<Guid, IEntity> Entities { get; }
        private Dictionary<Type, List<IEntity>> EntitiesByComponent { get; }
        private Dictionary<Type, int> ComponentCountByType { get; }

        public EntityManager()
        {
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
        public void RegisterEntity(IEntity entity)
        {
            if (entity == null)
            {
                throw new NullReferenceException(nameof(entity));
            }

            Entities.Add(entity.ID, entity);

            foreach (Type type in entity.ComponentTypes)
            {
                if (!typeof(IComponent).IsAssignableFrom(type))
                {
                    throw new TypeLoadException($"Component types must be assignable from {nameof(IComponent)}.");
                }

                RegisterComponent(entity, entity.GetComponent(type));
            }

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
        public void RegisterComponent(IEntity entity, IComponent component) =>
            RegisterComponentInternal(entity, component.GetType(), component);

        /// <summary>
        ///     Adds the specified component to the given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to add component to.</param>
        /// <typeparam name="T">Type of component to add.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RegisterComponent<T>(IEntity entity) where T : IComponent =>
            RegisterComponentInternal(entity, typeof(T), null);

        private void RegisterComponentInternal(IEntity entity, Type type, IComponent? component)
        {
            IComponent instance = component ?? (IComponent)Activator.CreateInstance(type);

            Debug.Assert(instance != null);

            if (!entity.TryAddComponent(instance))
            {
                return;
            }

            List<Type> implementedTypes = new List<Type>
            {
                type
            };
            implementedTypes.AddRange(type.GetInterfaces().Where(interfaceType => typeof(IComponent).IsAssignableFrom(interfaceType)));

            foreach (Type implementedType in implementedTypes)
            {
                if (!EntitiesByComponent.ContainsKey(implementedType))
                {
                    EntitiesByComponent.Add(implementedType, new List<IEntity>());
                }

                if (!ComponentCountByType.ContainsKey(implementedType))
                {
                    ComponentCountByType.Add(implementedType, 0);
                }

                EntitiesByComponent[implementedType].Add(entity);
                ComponentCountByType[implementedType] += 1;
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
        public void RemoveComponent<T>(IEntity entity) where T : IComponent
        {
            if (entity.TryRemoveComponent<T>())
            {
                EntitiesByComponent[typeof(T)].Remove(entity);
            }
        }

        #endregion

        #region Get .. Data

        /// <summary>
        ///     Returns <see cref="IEnumerable{T}" /> of entities containing component <see cref="T1" />.
        /// </summary>
        /// <typeparam name="T1"><see cref="IComponent" /> type to retrieve by.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of entities containing component <see cref="T1" />.</returns>
        /// <remarks>
        ///     Be cautious of registering or removing <see cref="IComponent" />s when iterating entities from this function, as
        ///     any
        ///     additions or subtractions from the collection will throw a collection modified exception.
        /// </remarks>
        public IEnumerable<IEntity> GetEntitiesWithComponents<T1>() where T1 : IComponent =>
            !EntitiesByComponent.ContainsKey(typeof(T1)) ? Enumerable.Empty<IEntity>() : EntitiesByComponent[typeof(T1)];


        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2>()
            where T1 : IComponent
            where T2 : IComponent
        {
            HashSet<IEntity> matchingEntityIDs = new HashSet<IEntity>(GetEntitiesWithComponents<T1>());
            matchingEntityIDs.IntersectWith(GetEntitiesWithComponents<T2>());

            return matchingEntityIDs;
        }

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
        {
            HashSet<IEntity> matchingEntityIDs = new HashSet<IEntity>(GetEntitiesWithComponents<T1>());
            matchingEntityIDs.IntersectWith(GetEntitiesWithComponents<T2>());
            matchingEntityIDs.IntersectWith(GetEntitiesWithComponents<T3>());

            return matchingEntityIDs;
        }

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3, T4>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent
        {
            HashSet<IEntity> matchingEntityIDs = new HashSet<IEntity>(GetEntitiesWithComponents<T1>());
            matchingEntityIDs.IntersectWith(GetEntitiesWithComponents<T2>());
            matchingEntityIDs.IntersectWith(GetEntitiesWithComponents<T3>());
            matchingEntityIDs.IntersectWith(GetEntitiesWithComponents<T4>());

            return matchingEntityIDs;
        }

        /// <summary>
        ///     Returns all instances of components of type <see cref="T1" />
        /// </summary>
        /// <typeparam name="T1">Type of <see cref="IComponent" /> to return.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of all instances of <see cref="IComponent" /> type <see cref="T1" />.</returns>
        public IEnumerable<T1> GetComponents<T1>()
            where T1 : IComponent
        {
            if (!EntitiesByComponent.ContainsKey(typeof(T1)))
            {
                yield break;
            }

            foreach (IEntity entity in EntitiesByComponent[typeof(T1)])
            {
                yield return entity.GetComponent<T1>();
            }
        }

        public IEnumerable<(T1, T2)> GetComponents<T1, T2>()
            where T1 : IComponent
            where T2 : IComponent
        {
            if (!EntitiesByComponent.ContainsKey(typeof(T1)))
            {
                yield break;
            }
            else if (!EntitiesByComponent.ContainsKey(typeof(T2)))
            {
                yield break;
            }

            foreach (IEntity entity in GetEntitiesWithComponents<T1, T2>())
            {
                yield return (entity.GetComponent<T1>(), entity.GetComponent<T2>());
            }
        }

        public IEnumerable<(T1, T2, T3)> GetComponents<T1, T2, T3>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
        {
            if (!EntitiesByComponent.ContainsKey(typeof(T1)))
            {
                yield break;
            }
            else if (!EntitiesByComponent.ContainsKey(typeof(T2)))
            {
                yield break;
            }
            else if (!EntitiesByComponent.ContainsKey(typeof(T3)))
            {
                yield break;
            }

            foreach (IEntity entity in GetEntitiesWithComponents<T1, T2, T3>())
            {
                yield return (entity.GetComponent<T1>(), entity.GetComponent<T2>(), entity.GetComponent<T3>());
            }
        }

        public IEnumerable<(T1, T2, T3, T4)> GetComponents<T1, T2, T3, T4>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent
        {
            if (!EntitiesByComponent.ContainsKey(typeof(T1)))
            {
                yield break;
            }
            else if (!EntitiesByComponent.ContainsKey(typeof(T2)))
            {
                yield break;
            }
            else if (!EntitiesByComponent.ContainsKey(typeof(T3)))
            {
                yield break;
            }
            else if (!EntitiesByComponent.ContainsKey(typeof(T4)))
            {
                yield break;
            }

            foreach (IEntity entity in GetEntitiesWithComponents<T1, T2, T3, T4>())
            {
                yield return (entity.GetComponent<T1>(), entity.GetComponent<T2>(), entity.GetComponent<T3>(), entity.GetComponent<T4>());
            }
        }

        public int GetComponentCount<T>() where T : IComponent => ComponentCountByType[typeof(T)];

        public int GetComponentCount(Type type)
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
