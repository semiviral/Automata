#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Components;
using Serilog;

#endregion


namespace Automata.Engine.Entities
{
    public sealed class EntityManager
    {
        private readonly Dictionary<Type, int> _ComponentCountByType;
        private readonly List<IEntity> _Entities;

        public EntityManager()
        {
            _Entities = new List<IEntity>();
            _ComponentCountByType = new Dictionary<Type, int>();
        }

        public IEntity ComposeEntity<TEntityComposition>(bool autoRegister) where TEntityComposition : IEntityComposition, new()
        {
            IEntity entity = new Entity();

            foreach (Type type in new TEntityComposition().ComposedTypes)
            {
                IComponent? component = (IComponent?)Activator.CreateInstance(type);

                if (component is null) throw new InvalidOperationException("Types used for composition must implement a parameterless constructor.");

                entity.AddComponent(component);
            }

            if (autoRegister) RegisterEntity(entity);

            return entity;
        }

        private void AddEntityInternal(IEntity entity)
        {
            _Entities.Add(entity);

            foreach (Type type in entity.Components.Keys) RegisterComponent(entity, entity.GetComponent(type));
        }

        private void RemoveEntityInternal(IEntity entity)
        {
            _Entities.Remove(entity);

            foreach (Type type in entity.Components.Keys) RemoveComponent(entity, type);
        }


        #region Remove .. Data

        public void RemoveEntity(IEntity entity) => RemoveEntityInternal(entity);

        /// <summary>
        ///     Removes the specified component instance from given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to remove component from.</param>
        /// <typeparam name="TComponent">Type of component to remove.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RemoveComponent<TComponent>(IEntity entity) where TComponent : class, IComponent => RemoveComponentInternal(entity, typeof(TComponent));

        public void RemoveComponent(IEntity entity, Type type) => RemoveComponentInternal(entity, type);

        private void RemoveComponentInternal(IEntity entity, Type type)
        {
            if (!typeof(IComponent).IsAssignableFrom(type)) throw new TypeLoadException($"Component types must be assignable from {nameof(IComponent)}.");

            IComponent component = entity.RemoveComponent(type);
            _ComponentCountByType[type] -= 1;

            if (component is IDisposable disposable) disposable.Dispose();
        }

        #endregion


        #region Register .. Data

        /// <summary>
        ///     Registers given entity with <see cref="EntityManager" />.
        /// </summary>
        /// <param name="entity">Entity to register.</param>
        /// <exception cref="NullReferenceException">Thrown when given <see cref="IEntity" /> is null.</exception>
        public void RegisterEntity(IEntity entity)
        {
            AddEntityInternal(entity);

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
        public void RegisterComponent<T>(IEntity entity) where T : class, IComponent =>
            RegisterComponentInternal(entity, typeof(T), null);

        private void RegisterComponentInternal(IEntity entity, Type type, IComponent? component)
        {
            if (!typeof(IComponent).IsAssignableFrom(type)) throw new TypeLoadException($"Component types must be assignable from {nameof(IComponent)}.");

            IComponent? instance = component ?? (IComponent?)Activator.CreateInstance(type);

            if (instance is null) throw new TypeLoadException($"Failed to initialize {nameof(IComponent)} of type '{type.FullName}'.");
            else if (!entity.TryGetComponent(type, out _)) entity.AddComponent(instance);

            if (!_ComponentCountByType.ContainsKey(type)) _ComponentCountByType.Add(type, 0);

            _ComponentCountByType[type] += 1;
        }

        #endregion


        #region Get .. Data

        public IEnumerable<TComponent> GetComponentsAssignableFrom<TComponent>() where TComponent : class, IComponent
        {
            foreach (IComponent component in _Entities.SelectMany(entity => entity.Components.Values))
                if (component is TComponent componentT)
                    yield return componentT;
        }

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
            where T1 : class, IComponent =>
            _Entities.Where(entity => entity.HasComponent<T1>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2>()
            where T1 : class, IComponent
            where T2 : class, IComponent =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>())
                .Intersect(GetEntitiesWithComponents<T3>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3, T4>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent =>
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
            where T1 : class, IComponent
        {
            foreach (IEntity entity in _Entities)
            {
                if (entity.TryGetComponent(out T1? component)) yield return component;
            }
        }

        public IEnumerable<(T1, T2)> GetComponents<T1, T2>()
            where T1 : class, IComponent
            where T2 : class, IComponent
        {
            foreach (IEntity entity in _Entities)
            {
                if (entity.TryGetComponent(out T1? component)
                    && entity.TryGetComponent(out T2? component2)) yield return (component, component2);
            }
        }

        public IEnumerable<(T1, T2, T3)> GetComponents<T1, T2, T3>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
        {
            foreach (IEntity entity in _Entities)
            {
                if (entity.TryGetComponent(out T1? component)
                    && entity.TryGetComponent(out T2? component2)
                    && entity.TryGetComponent(out T3? component3)) yield return (component, component2, component3);
            }
        }

        public IEnumerable<(T1, T2, T3, T4)> GetComponents<T1, T2, T3, T4>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent
        {
            foreach (IEntity entity in _Entities)
            {
                if (entity.TryGetComponent(out T1? component)
                    && entity.TryGetComponent(out T2? component2)
                    && entity.TryGetComponent(out T3? component3)
                    && entity.TryGetComponent(out T4? component4)) yield return (component, component2, component3, component4);
            }
        }

        public int GetComponentCount<TComponent>() where TComponent : class, IComponent =>
            _ComponentCountByType.TryGetValue(typeof(TComponent), out int count) ? count : 0;

        public int GetComponentCount(Type type) => _ComponentCountByType.TryGetValue(type, out int count) ? count : 0;

        #endregion
    }
}
