#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public sealed class EntityManager
    {
        private readonly Dictionary<Type, int> _ComponentCounts;
        private readonly List<IEntity> _Entities;

        public EntityManager()
        {
            _Entities = new List<IEntity>();
            _ComponentCounts = new Dictionary<Type, int>();
        }

        public IEntity CreateEntity(params Component[] components)
        {
            IEntity entity = new Entity(_Entities.Count);

            foreach (Component component in components)
            {
                entity.Add(component);
                Type type = component.GetType();

                if (!_ComponentCounts.ContainsKey(type)) _ComponentCounts.Add(type, 1);
                else _ComponentCounts[type] += 1;
            }

            _Entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(IEntity entity)
        {
            foreach (Component component in entity) _ComponentCounts[component.GetType()] -= 1;

            _Entities.Remove(entity);
            entity.Dispose();
        }


        #region Remove .. Data

        /// <summary>
        ///     Removes the specified component instance from given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to remove component from.</param>
        /// <typeparam name="TComponent">Type of component to remove.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RemoveComponent<TComponent>(IEntity entity) where TComponent : Component
        {
            entity.Remove<TComponent>();
            _ComponentCounts[typeof(TComponent)] -= 1;
        }

        #endregion


        #region Register .. Data

        /// <summary>
        ///     Registers the specified component instance to the given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to add component to.</param>
        /// <param name="component">Component to add.</param>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RegisterComponent(IEntity entity, Component component)
        {
            entity.Add(component);
            Type type = component.GetType();

            if (!_ComponentCounts.ContainsKey(type)) _ComponentCounts.Add(type, 1);
            else _ComponentCounts[type] += 1;
        }

        /// <summary>
        ///     Adds the specified component to the given <see cref="Entity" />.
        /// </summary>
        /// <param name="entity"><see cref="Entity" /> to add component to.</param>
        /// <typeparam name="TComponent">Type of component to add.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RegisterComponent<TComponent>(IEntity entity) where TComponent : Component, new()
        {
            entity.Add<TComponent>();

            if (!_ComponentCounts.ContainsKey(typeof(TComponent))) _ComponentCounts.Add(typeof(TComponent), 1);
            else _ComponentCounts[typeof(TComponent)] += 1;
        }

        #endregion


        #region Get .. Data

        public IEnumerable<TComponent> GetComponentsExplicit<TComponent>() where TComponent : Component
        {
            foreach (IEntity entity in _Entities)
            foreach (Component component in entity)
                if (component is TComponent componentT)
                    yield return componentT;
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
        public IEnumerable<IEntity> GetEntities<T1>()
            where T1 : Component =>
            _Entities.Where(entity => entity.Contains<T1>());

        public IEnumerable<IEntity> GetEntities<T1, T2>()
            where T1 : Component
            where T2 : Component =>
            GetEntities<T1>()
                .Intersect(GetEntities<T2>());

        public IEnumerable<IEntity> GetEntities<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component =>
            GetEntities<T1>()
                .Intersect(GetEntities<T2>())
                .Intersect(GetEntities<T3>());

        public IEnumerable<IEntity> GetEntities<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component =>
            GetEntities<T1>()
                .Intersect(GetEntities<T2>())
                .Intersect(GetEntities<T3>())
                .Intersect(GetEntities<T4>());

        public IEnumerable<(IEntity Entity, T1 Component1)> GetEntitiesWithComponents<T1>()
            where T1 : Component
        {
            foreach (IEntity entity in _Entities)
                if (entity.TryFind(out T1? component1))
                    yield return (entity, component1);
        }

        public IEnumerable<(IEntity Entity, T1 Component1, T2 Component2)> GetEntitiesWithComponents<T1, T2>()
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
            _ComponentCounts.TryGetValue(typeof(TComponent), out int count) ? count : 0;

        public int GetComponentCount(Type type) => _ComponentCounts.TryGetValue(type, out int count) ? count : 0;

        #endregion
    }
}
