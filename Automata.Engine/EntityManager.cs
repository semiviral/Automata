using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf

namespace Automata.Engine
{
    public sealed class EntityManager : IDisposable
    {
        private readonly DerivedSet<HashSet<Entity>> _ComponentCache;

        public int EntityCount { get; set; }

        public EntityManager() => _ComponentCache = new DerivedSet<HashSet<Entity>>();


        #region Entity Create / Remove

        public Entity CreateEntity(params Component[] components)
        {
            Entity entity = new Entity();

            foreach (Component component in components)
            {
                RegisterComponent(entity, component);
            }

            EntityCount += 1;
            return entity;
        }

        public void RemoveEntity(Entity entity)
        {
            foreach (Component component in entity)
            {
                if (_ComponentCache.TryGetItem(component.GetType(), out HashSet<Entity>? entities))
                {
                    entities!.Remove(entity);
                }
            }

            EntityCount -= 1;
            entity.Dispose();
        }

        #endregion


        #region Component Register / Remove

        /// <summary>
        ///     Registers the specified component instance to the given <see cref="Entity" />.
        /// </summary>
        /// <param name="entity"><see cref="Entity" /> to add component to.</param>
        /// <param name="component">Component to add.</param>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RegisterComponent(Entity entity, Component component)
        {
            entity.Add(component);

            _ComponentCache.TryAdd(component.GetType(), new HashSet<Entity>());
            _ComponentCache[component.GetType()].Add(entity);
        }

        /// <summary>
        ///     Adds the specified component to the given <see cref="Entity" />.
        /// </summary>
        /// <param name="entity"><see cref="Entity" /> to add component to.</param>
        /// <typeparam name="TComponent">Type of component to add.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public TComponent RegisterComponent<TComponent>(Entity entity) where TComponent : Component, new()
        {
            TComponent component = entity.Add<TComponent>().Unwrap();

            _ComponentCache.TryAdd(typeof(TComponent), new HashSet<Entity>());
            _ComponentCache[typeof(TComponent)].Add(entity);

            return component;
        }

        /// <summary>
        ///     Removes the specified component instance from given <see cref="Entity" />.
        /// </summary>
        /// <param name="entity"><see cref="Entity" /> to remove component from.</param>
        /// <typeparam name="TComponent">Type of component to remove.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public void RemoveComponent<TComponent>(Entity entity) where TComponent : Component
        {
            entity.Remove<TComponent>();

            if (_ComponentCache.TryGetItem(typeof(TComponent), out HashSet<Entity>? entities))
            {
                entities!.Remove(entity);

                if (entities.Count is 0)
                {
                    _ComponentCache.Remove(typeof(TComponent));
                }
            }
        }

        #endregion


        #region GetEntities

        /// <summary>
        ///     Returns <see cref="IEnumerable{T}" /> of entities containing component <see cref="T1" />.
        /// </summary>
        /// <typeparam name="T1"><see cref="Component" /> type to retrieve by.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of entities containing component <see cref="T1" />.</returns>
        /// <remarks>
        ///     Be cautious of registering or removing <see cref="Component" />s when iterating entities from this function, as
        ///     any additions or subtractions from the collection will throw a collection modified exception.
        /// </remarks>
        public IEnumerable<Entity> GetEntities<T1>()
            where T1 : Component
        {
            if (_ComponentCache.TryGetItem(typeof(T1), out HashSet<Entity>? entities1))
            {
                return entities1!;
            }

            return Enumerable.Empty<Entity>();
        }

        public IEnumerable<Entity> GetEntities<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            if (_ComponentCache.TryGetItem(typeof(T1), out HashSet<Entity>? entities1)
                && _ComponentCache.TryGetItem(typeof(T2), out HashSet<Entity>? entities2))
            {
                foreach (Entity entity in entities1!)
                {
                    if (entities2!.Contains(entity))
                    {
                        yield return entity;
                    }
                }
            }
        }

        public IEnumerable<Entity> GetEntities<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            if (_ComponentCache.TryGetItem(typeof(T1), out HashSet<Entity>? entities1)
                && _ComponentCache.TryGetItem(typeof(T2), out HashSet<Entity>? entities2)
                && _ComponentCache.TryGetItem(typeof(T3), out HashSet<Entity>? entities3))
            {
                foreach (Entity entity in entities1!)
                {
                    if (entities2!.Contains(entity) && entities3!.Contains(entity))
                    {
                        yield return entity;
                    }
                }
            }
        }

        public IEnumerable<Entity> GetEntities<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            if (_ComponentCache.TryGetItem(typeof(T1), out HashSet<Entity>? entities1)
                && _ComponentCache.TryGetItem(typeof(T2), out HashSet<Entity>? entities2)
                && _ComponentCache.TryGetItem(typeof(T3), out HashSet<Entity>? entities3)
                && _ComponentCache.TryGetItem(typeof(T4), out HashSet<Entity>? entities4))
            {
                foreach (Entity entity in entities1!)
                {
                    if (entities2!.Contains(entity) && entities3!.Contains(entity) && entities4!.Contains(entity))
                    {
                        yield return entity;
                    }
                }
            }
        }

        #endregion


        #region GetEntitiesWithComponents

        public IEnumerable<(Entity Entity, T1 Component1)> GetEntitiesWithComponents<T1>()
            where T1 : Component
        {
            foreach (Entity entity in GetEntities<T1>())
            {
                yield return (entity,
                    entity.Component<T1>().Unwrap());
            }
        }

        public IEnumerable<(Entity Entity, T1 Component1, T2 Component2)> GetEntitiesWithComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (Entity entity in GetEntities<T1, T2>())
            {
                yield return (entity,
                    entity.Component<T1>().Unwrap(),
                    entity.Component<T2>().Unwrap());
            }
        }

        public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3)> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            foreach (Entity entity in GetEntities<T1, T2, T3>())
            {
                yield return (entity,
                    entity.Component<T1>().Unwrap(),
                    entity.Component<T2>().Unwrap(),
                    entity.Component<T3>().Unwrap());
            }
        }

        public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4)> GetEntitiesWithComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            foreach (Entity entity in GetEntities<T1, T2, T3, T4>())
            {
                yield return (entity,
                    entity.Component<T1>().Unwrap(),
                    entity.Component<T2>().Unwrap(),
                    entity.Component<T3>().Unwrap(),
                    entity.Component<T4>().Unwrap());
            }
        }

        #endregion


        #region GetComponents

        /// <summary>
        ///     Returns all instances of components of type <see cref="T1" />
        /// </summary>
        /// <typeparam name="T1">Type of <see cref="Component" /> to return.</typeparam>
        /// <returns><see cref="IEnumerable{T}" /> of all instances of <see cref="Component" /> type <see cref="T1" />.</returns>
        public IEnumerable<T1> GetComponents<T1>()
            where T1 : Component
        {
            foreach (Entity entity in GetEntities<T1>())
            {
                yield return
                    entity.Component<T1>().Unwrap();
            }
        }

        public IEnumerable<(T1, T2)> GetComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (Entity entity in GetEntities<T1, T2>())
            {
                yield return
                    (entity.Component<T1>().Unwrap(),
                        entity.Component<T2>().Unwrap());
            }
        }

        public IEnumerable<(T1, T2, T3)> GetComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            foreach (Entity entity in GetEntities<T1, T2, T3>())
            {
                yield return
                    (entity.Component<T1>().Unwrap(),
                        entity.Component<T2>().Unwrap(),
                        entity.Component<T3>().Unwrap());
            }
        }

        public IEnumerable<(T1, T2, T3, T4)> GetComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            foreach (Entity entity in GetEntities<T1, T2, T3, T4>())
            {
                yield return
                    (entity.Component<T1>().Unwrap(),
                    entity.Component<T2>().Unwrap(),
                    entity.Component<T3>().Unwrap(),
                    entity.Component<T4>().Unwrap());
            }
        }

        #endregion


        #region Component Count

        public nint GetComponentCount(Type type) => _ComponentCache.TryGetItem(type, out HashSet<Entity>? entities) ? entities!.Count : 0;

        public nint GetComponentCount<TComponent>() where TComponent : Component =>
            _ComponentCache.TryGetItem(typeof(TComponent), out HashSet<Entity>? entities) ? entities!.Count : 0;

        #endregion


        #region IDisposable

        public void Dispose()
        {
            foreach ((_, HashSet<Entity> entities) in _ComponentCache)
            {
                foreach (Entity entity in entities)
                {
                    entity.Dispose();
                }
            }
        }

        #endregion
    }
}
