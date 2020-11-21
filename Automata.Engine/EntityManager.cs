using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Automata.Engine.Collections;

namespace Automata.Engine
{
    public sealed class EntityManager : IDisposable
    {
        private readonly Dictionary<Type, IEnumerable> _CachedEnumerators;
        private readonly Dictionary<Type, nint> _ComponentCounts;
        private readonly NonAllocatingList<IEntity> _Entities;

        public nint EntityCount => _Entities.Count;

        public EntityManager()
        {
            _Entities = new NonAllocatingList<IEntity>();
            _ComponentCounts = new Dictionary<Type, nint>();
            _CachedEnumerators = new Dictionary<Type, IEnumerable>();
        }

        private IEnumerable<T> CacheAndGetEnumerable<T>(IEnumerable<T> setter)
        {
            if (!_CachedEnumerators.TryGetValue(typeof(T), out IEnumerable? enumerable))
            {
                enumerable = setter;
                _CachedEnumerators.Add(typeof(T), enumerable);
            }

            Debug.Assert(enumerable is IEnumerable<T>);
            return (enumerable as IEnumerable<T>)!;
        }

        public IEnumerable<TComponent> GetComponentsExplicit<TComponent>() where TComponent : Component
        {
            IEnumerable<TComponent> GetComponentsExplicitImpl()
            {
                foreach (IEntity entity in _Entities)
                foreach (Component component in entity)
                {
                    if (component is TComponent componentT)
                    {
                        yield return componentT;
                    }
                }
            }

            return CacheAndGetEnumerable(GetComponentsExplicitImpl());
        }


        #region IDisposable

        public void Dispose()
        {
            foreach (IEntity entity in _Entities)
            {
                entity.Dispose();
            }

            _Entities.Dispose();
        }

        #endregion


        #region Entity Create / Remove

        public IEntity CreateEntity(params Component[] components)
        {
            IEntity entity = new Entity();

            foreach (Component component in components)
            {
                entity.Add(component);
                IncrementComponentCount(component);
            }

            _Entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(IEntity entity)
        {
            foreach (Component component in entity)
            {
                DecrementComponentCount(component);
            }

            _Entities.Remove(entity);

            // entity will dispose its own components
            entity.Dispose();
        }

        #endregion


        #region Component Register / Remove

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
            IncrementComponentCount(component);
        }

        /// <summary>
        ///     Adds the specified component to the given <see cref="IEntity" />.
        /// </summary>
        /// <param name="entity"><see cref="IEntity" /> to add component to.</param>
        /// <typeparam name="TComponent">Type of component to add.</typeparam>
        /// <remarks>
        ///     Use this method to ensure <see cref="EntityManager" /> caches remain accurate.
        /// </remarks>
        public TComponent RegisterComponent<TComponent>(IEntity entity) where TComponent : Component, new()
        {
            TComponent component = entity.Add<TComponent>();
            IncrementComponentCount<TComponent>();

            return component;
        }

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
            DecrementComponentCount<TComponent>();
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
        public IEnumerable<IEntity> GetEntities<T1>()
            where T1 : Component
        {
            IEnumerable<IEntity> GetEntitiesImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? _))
                    {
                        yield return entity;
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesImpl());
        }

        public IEnumerable<IEntity> GetEntities<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            IEnumerable<IEntity> GetEntitiesImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? _)
                        && entity.TryFind(out T2? _))
                    {
                        yield return entity;
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesImpl());
        }

        public IEnumerable<IEntity> GetEntities<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            IEnumerable<IEntity> GetEntitiesImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? _)
                        && entity.TryFind(out T2? _)
                        && entity.TryFind(out T3? _))
                    {
                        yield return entity;
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesImpl());
        }

        public IEnumerable<IEntity> GetEntities<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            IEnumerable<IEntity> GetEntitiesImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? _)
                        && entity.TryFind(out T2? _)
                        && entity.TryFind(out T3? _)
                        && entity.TryFind(out T4? _))
                    {
                        yield return entity;
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesImpl());
        }

        #endregion


        #region GetEntitiesWithComponents

        public IEnumerable<(IEntity Entity, T1 Component1)> GetEntitiesWithComponents<T1>()
            where T1 : Component
        {
            IEnumerable<(IEntity, T1)> GetEntitiesWithComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component1))
                    {
                        yield return (entity, component1);
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesWithComponentsImpl());
        }

        public IEnumerable<(IEntity Entity, T1 Component1, T2 Component2)> GetEntitiesWithComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            IEnumerable<(IEntity, T1, T2)> GetEntitiesWithComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component1)
                        && entity.TryFind(out T2? component2))
                    {
                        yield return (entity, component1, component2);
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesWithComponentsImpl());
        }

        public IEnumerable<(IEntity Entity, T1 Component1, T2 Component2, T3 Component3)> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            IEnumerable<(IEntity, T1, T2, T3)> GetEntitiesWithComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component1)
                        && entity.TryFind(out T2? component2)
                        && entity.TryFind(out T3? component3))
                    {
                        yield return (entity, component1, component2, component3);
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesWithComponentsImpl());
        }

        public IEnumerable<(IEntity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4)> GetEntitiesWithComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            IEnumerable<(IEntity, T1, T2, T3, T4)> GetEntitiesWithComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component1)
                        && entity.TryFind(out T2? component2)
                        && entity.TryFind(out T3? component3)
                        && entity.TryFind(out T4? component4))
                    {
                        yield return (entity, component1, component2, component3, component4);
                    }
                }
            }

            return CacheAndGetEnumerable(GetEntitiesWithComponentsImpl());
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
            IEnumerable<T1> GetComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component))
                    {
                        yield return component;
                    }
                }
            }

            return CacheAndGetEnumerable(GetComponentsImpl());
        }

        public IEnumerable<(T1, T2)> GetComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            IEnumerable<(T1, T2)> GetComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component)
                        && entity.TryFind(out T2? component2))
                    {
                        yield return (component, component2);
                    }
                }
            }

            return CacheAndGetEnumerable(GetComponentsImpl());
        }

        public IEnumerable<(T1, T2, T3)> GetComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            IEnumerable<(T1, T2, T3)> GetComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component)
                        && entity.TryFind(out T2? component2)
                        && entity.TryFind(out T3? component3))
                    {
                        yield return (component, component2, component3);
                    }
                }
            }

            return CacheAndGetEnumerable(GetComponentsImpl());
        }

        public IEnumerable<(T1, T2, T3, T4)> GetComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            IEnumerable<(T1, T2, T3, T4)> GetComponentsImpl()
            {
                foreach (IEntity entity in _Entities)
                {
                    if (entity.TryFind(out T1? component)
                        && entity.TryFind(out T2? component2)
                        && entity.TryFind(out T3? component3)
                        && entity.TryFind(out T4? component4))
                    {
                        yield return (component, component2, component3, component4);
                    }
                }
            }

            return CacheAndGetEnumerable(GetComponentsImpl());
        }

        #endregion


        #region Component Count

        private void IncrementComponentCount(Component component)
        {
            Type type = component.GetType();

            if (!_ComponentCounts.TryAdd(type, 1))
            {
                _ComponentCounts[type] += 1;
            }
        }

        private void IncrementComponentCount<TComponent>() where TComponent : Component
        {
            if (!_ComponentCounts.TryAdd(typeof(TComponent), 1))
            {
                _ComponentCounts[typeof(TComponent)] += 1;
            }
        }

        private void DecrementComponentCount(Component component)
        {
            Type type = component.GetType();
            _ComponentCounts[type] -= 1;

            Debug.Assert(_ComponentCounts[type] >= 0, $"{nameof(EntityManager)} component count for '{type.FullName}' is in an invalid state.");
        }

        private void DecrementComponentCount<TComponent>() where TComponent : Component
        {
            _ComponentCounts[typeof(TComponent)] -= 1;

            Debug.Assert(_ComponentCounts[typeof(TComponent)] >= 0,
                $"{nameof(EntityManager)} component count for '{typeof(TComponent).FullName}' is in an invalid state.");
        }

        public nint GetComponentCount(Type type) => _ComponentCounts.TryGetValue(type, out nint count) ? count : 0;

        public nint GetComponentCount<TComponent>() where TComponent : Component =>
            _ComponentCounts.TryGetValue(typeof(TComponent), out nint count) ? count : 0;

        #endregion
    }
}
