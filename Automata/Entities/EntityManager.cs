#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Components;
using Automata.Exceptions;
using Serilog;

#endregion

namespace Automata.Entities
{
    public class EntityManager
    {
        private readonly Dictionary<Guid, IEntity> _Entities;
        private readonly Dictionary<Type, List<IEntity>> _EntitiesByComponent;
        private readonly Dictionary<Type, int> _ComponentCountByType;

        public EntityManager()
        {
            _Entities = new Dictionary<Guid, IEntity>();
            _EntitiesByComponent = new Dictionary<Type, List<IEntity>>();
            _ComponentCountByType = new Dictionary<Type, int>();
        }

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
            entity.RemoveComponent<T>();

            _EntitiesByComponent[typeof(T)].Remove(entity);
            _ComponentCountByType.Remove(typeof(T));
        }

        #endregion

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

            _Entities.Add(entity.ID, entity);

            foreach (Type type in entity.ComponentTypes)
            {
                if (!typeof(IComponent).IsAssignableFrom(type))
                {
                    throw new TypeLoadException($"Component types must be assignable from {nameof(IComponent)}.");
                }

                RegisterComponent(entity, entity.GetComponent(type));
            }

            Log.Verbose($"({nameof(EntityManager)}) Registered {nameof(IEntity)}: {entity.ID}");
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
            IComponent? instance = component ?? (IComponent?)Activator.CreateInstance(type);

            if (instance is null)
            {
                throw new TypeLoadException($"Failed to initialize {nameof(IComponent)} of type '{type.FullName}'.");
            }
            else
            {
                entity.AddComponent(instance);
            }

            if (!_EntitiesByComponent.ContainsKey(type))
            {
                _EntitiesByComponent.Add(type, new List<IEntity>());
            }

            if (!_ComponentCountByType.ContainsKey(type))
            {
                _ComponentCountByType.Add(type, 0);
            }

            _EntitiesByComponent[type].Add(entity);
            _ComponentCountByType[type] += 1;
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
        ///     any additions or subtractions from the collection will throw a collection modified exception.
        /// </remarks>
        public IEnumerable<IEntity> GetEntitiesWithComponents<T1>()
            where T1 : IComponent =>
            _EntitiesByComponent.TryGetValue(typeof(T1), out List<IEntity>? entities)
                ? entities
                : Enumerable.Empty<IEntity>();


        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2>()
            where T1 : IComponent
            where T2 : IComponent =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>())
                .Intersect(GetEntitiesWithComponents<T3>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3, T4>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>())
                .Intersect(GetEntitiesWithComponents<T3>())
                .Intersect(GetEntitiesWithComponents<T4>());

        /// <summary>
        ///     Returns all instances of components of type <see cref="T1" />
        /// </summary>
        /// <typeparam name="T1">Type of <see cref="IComponent" /> to return.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of all instances of <see cref="IComponent" /> type <see cref="T1" />.</returns>
        public IEnumerable<T1> GetComponents<T1>()
            where T1 : IComponent =>
            GetEntitiesWithComponents<T1>().Select(entity =>
                entity.GetComponent<T1>());

        public IEnumerable<(T1, T2)> GetComponents<T1, T2>()
            where T1 : IComponent
            where T2 : IComponent =>
            GetEntitiesWithComponents<T1, T2>().Select(entity =>
                (entity.GetComponent<T1>(), entity.GetComponent<T2>()));

        public IEnumerable<(T1, T2, T3)> GetComponents<T1, T2, T3>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent =>
            GetEntitiesWithComponents<T1, T2, T3>().Select(entity =>
                (entity.GetComponent<T1>(), entity.GetComponent<T2>(), entity.GetComponent<T3>()));

        public IEnumerable<(T1, T2, T3, T4)> GetComponents<T1, T2, T3, T4>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent =>
            GetEntitiesWithComponents<T1, T2, T3, T4>().Select(entity =>
                (entity.GetComponent<T1>(), entity.GetComponent<T2>(), entity.GetComponent<T3>(), entity.GetComponent<T4>()));

        public int GetComponentCount<T>() where T : IComponent => _ComponentCountByType[typeof(T)];

        public int GetComponentCount(Type type)
        {
            if (!typeof(IComponent).IsAssignableFrom(type))
            {
                throw new TypeLoadException(type.ToString());
            }

            if (_ComponentCountByType.TryGetValue(type, out int count))
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
