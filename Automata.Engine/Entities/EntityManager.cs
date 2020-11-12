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
                Component? component = (Component?)Activator.CreateInstance(type);

                if (component is null) throw new InvalidOperationException("Types used for composition must implement a parameterless constructor.");

                entity.Add(component);
            }

            if (autoRegister) RegisterEntity(entity);

            return entity;
        }

        private void AddEntityInternal(IEntity entity)
        {
            _Entities.Add(entity);

            foreach (Component component in entity)
            {
                Type type = component.GetType();

                if (!_ComponentCountByType.ContainsKey(type)) _ComponentCountByType.Add(type, 1);
                else _ComponentCountByType[type] += 1;
            }
        }

        private void RemoveEntityInternal(IEntity entity)
        {
            _Entities.Remove(entity);

            foreach (Component component in entity)
            {
                if (component is IDisposable disposable) disposable.Dispose();

                _ComponentCountByType[component.GetType()] -= 1;
            }

            entity.Destroy();
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
        public void RemoveComponent<TComponent>(IEntity entity) where TComponent : Component => RemoveComponentInternal(entity, typeof(TComponent));

        public void RemoveComponent(IEntity entity, Type type) => RemoveComponentInternal(entity, type);

        private void RemoveComponentInternal(IEntity entity, Type type)
        {
            if (!typeof(Component).IsAssignableFrom(type)) throw new TypeLoadException($"Component types must be assignable from {nameof(Component)}.");

            if (entity.TryFind(type, out Component? component) && entity.Remove(component))
            {
                if (component is IDisposable disposable) disposable.Dispose();
                _ComponentCountByType[type] -= 1;
            }
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
        public void RegisterComponent(IEntity entity, Component component) =>
            RegisterComponentInternal(entity, component.GetType(), component);

        /// <summary>
        ///     Adds the specified component to the given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to add component to.</param>
        /// <typeparam name="T">Type of component to add.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RegisterComponent<T>(IEntity entity) where T : Component, new() =>
            RegisterComponentInternal(entity, typeof(T), null);

        private void RegisterComponentInternal(IEntity entity, Type type, Component? component)
        {
            if (!typeof(Component).IsAssignableFrom(type)) throw new TypeLoadException($"Component types must be assignable from {nameof(Component)}.");

            Component? instance = component ?? (Component?)Activator.CreateInstance(type);

            if (instance is null) throw new TypeLoadException($"Failed to initialize {nameof(Component)} of type '{type.FullName}'.");
            else if (!entity.TryFind(type, out _)) entity.Add(instance);

            if (!_ComponentCountByType.ContainsKey(type)) _ComponentCountByType.Add(type, 0);

            _ComponentCountByType[type] += 1;
        }

        #endregion


        #region Get .. Data

        public IEnumerable<TComponent> GetComponentsExplicit<TComponent>() where TComponent : Component
        {
            foreach (IEntity entity in _Entities)
            foreach (Component component in entity)
                if (component is TComponent tComponent)
                    yield return tComponent;
        }

        /// <summary>
        ///     Returns <see cref="IEnumerable{T}" /> of entities containing component <see cref="T1" />.
        /// </summary>
        /// <typeparam name="T1"><see cref="Component" /> type to retrieve by.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of entities containing component <see cref="T1" />.</returns>
        /// <remarks>
        ///     Be cautious of registering or removing <see cref="Component" />s when iterating entities from this function, as
        ///     any additions or subtractions from the collection will throw a collection modified exception.
        /// </remarks>
        public IEnumerable<IEntity> GetEntitiesWithComponents<T1>()
            where T1 : Component =>
            _Entities.Where(entity => entity.Contains<T1>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2>()
            where T1 : Component
            where T2 : Component =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>())
                .Intersect(GetEntitiesWithComponents<T3>());

        public IEnumerable<IEntity> GetEntitiesWithComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component =>
            GetEntitiesWithComponents<T1>()
                .Intersect(GetEntitiesWithComponents<T2>())
                .Intersect(GetEntitiesWithComponents<T3>())
                .Intersect(GetEntitiesWithComponents<T4>());

        public IEnumerable<(IEntity Entity, T1 Component1)> GetEntities<T1>()
            where T1 : Component
        {
            foreach (IEntity entity in _Entities)
                if (entity.TryFind(out T1? component1))
                    yield return (entity, component1);
        }

        public IEnumerable<(IEntity Entity, T1 Component1, T2 Component2)> GetEntities<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (IEntity entity in _Entities)
                if (entity.TryFind(out T1? component1)
                    && entity.TryFind(out T2? component2))
                    yield return (entity, component1, component2);
        }

        /// <summary>
        ///     Returns all instances of components of type <see cref="T1" />
        /// </summary>
        /// <typeparam name="T1">Type of <see cref="Component" /> to return.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of all instances of <see cref="Component" /> type <see cref="T1" />.</returns>
        public IEnumerable<T1> GetComponents<T1>()
            where T1 : Component
        {
            foreach (IEntity entity in _Entities)
                if (entity.TryFind(out T1? component))
                    yield return component;
        }

        public IEnumerable<(T1, T2)> GetComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (IEntity entity in _Entities)
                if (entity.TryFind(out T1? component)
                    && entity.TryFind(out T2? component2))
                    yield return (component, component2);
        }

        public IEnumerable<(T1, T2, T3)> GetComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            foreach (IEntity entity in _Entities)
                if (entity.TryFind(out T1? component)
                    && entity.TryFind(out T2? component2)
                    && entity.TryFind(out T3? component3))
                    yield return (component, component2, component3);
        }

        public IEnumerable<(T1, T2, T3, T4)> GetComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            foreach (IEntity entity in _Entities)
                if (entity.TryFind(out T1? component)
                    && entity.TryFind(out T2? component2)
                    && entity.TryFind(out T3? component3)
                    && entity.TryFind(out T4? component4))
                    yield return (component, component2, component3, component4);
        }

        public int GetComponentCount<TComponent>() where TComponent : Component =>
            _ComponentCountByType.TryGetValue(typeof(TComponent), out int count) ? count : 0;

        public int GetComponentCount(Type type) => _ComponentCountByType.TryGetValue(type, out int count) ? count : 0;

        #endregion
    }
}
