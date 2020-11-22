using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Automata.Engine.Collections;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace Automata.Engine
{
    public sealed class EntityManager : IDisposable
    {
        private readonly Dictionary<Type, IEnumerable> _CachedEnumerators;
        private readonly Dictionary<Type, nint> _ComponentCounts;
        private readonly NonAllocatingList<Entity> _Entities;

        public int EntityCount => _Entities.Count;

        public EntityManager()
        {
            _Entities = new NonAllocatingList<Entity>();
            _ComponentCounts = new Dictionary<Type, nint>();
            _CachedEnumerators = new Dictionary<Type, IEnumerable>();
        }

        public IEnumerable<TComponent> GetComponentsExplicit<TComponent>() where TComponent : Component
        {
            foreach (Entity entity in _Entities)
            foreach (Component component in entity)
            {
                if (component is TComponent componentT)
                {
                    yield return componentT;
                }
            }
        }


        #region Entity Create / Remove

        public Entity CreateEntity(params Component[] components)
        {
            Entity entity = new Entity();

            foreach (Component component in components)
            {
                entity.Add(component);
                IncrementComponentCount(component);
            }

            _Entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(Entity entity)
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
            IncrementComponentCount(component);
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
            TComponent component = entity.Add<TComponent>();
            IncrementComponentCount<TComponent>();

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
        public IEnumerable<Entity> GetEntities<T1>()
            where T1 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? _))
                {
                    yield return entity;
                }
            }
        }

        public IEnumerable<Entity> GetEntities<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? _)
                    && entity.TryFind(out T2? _))
                {
                    yield return entity;
                }
            }
        }

        public IEnumerable<Entity> GetEntities<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? _)
                    && entity.TryFind(out T2? _)
                    && entity.TryFind(out T3? _))
                {
                    yield return entity;
                }
            }
        }

        public IEnumerable<Entity> GetEntities<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            foreach (Entity entity in _Entities)
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

        #endregion


        #region GetEntitiesWithComponents

        public IEnumerable<(Entity Entity, T1 Component1)> GetEntitiesWithComponents<T1>()
            where T1 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? component1))
                {
                    yield return (entity, component1);
                }
            }
        }

        public IEnumerable<(Entity Entity, T1 Component1, T2 Component2)> GetEntitiesWithComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? component1)
                    && entity.TryFind(out T2? component2))
                {
                    yield return (entity, component1, component2);
                }
            }
        }

        public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3)> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? component1)
                    && entity.TryFind(out T2? component2)
                    && entity.TryFind(out T3? component3))
                {
                    yield return (entity, component1, component2, component3);
                }
            }
        }

        public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4)> GetEntitiesWithComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            foreach (Entity entity in _Entities)
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
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? component))
                {
                    yield return component;
                }
            }
        }

        public IEnumerable<(T1, T2)> GetComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? component)
                    && entity.TryFind(out T2? component2))
                {
                    yield return (component, component2);
                }
            }
        }

        public IEnumerable<(T1, T2, T3)> GetComponents<T1, T2, T3>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
        {
            foreach (Entity entity in _Entities)
            {
                if (entity.TryFind(out T1? component)
                    && entity.TryFind(out T2? component2)
                    && entity.TryFind(out T3? component3))
                {
                    yield return (component, component2, component3);
                }
            }
        }

        public IEnumerable<(T1, T2, T3, T4)> GetComponents<T1, T2, T3, T4>()
            where T1 : Component
            where T2 : Component
            where T3 : Component
            where T4 : Component
        {
            foreach (Entity entity in _Entities)
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

            Debug.Assert(_ComponentCounts[type] >= 0, $"{nameof(EntityManager)} component count for '{type.Name}' is in an invalid state.");
        }

        private void DecrementComponentCount<TComponent>() where TComponent : Component
        {
            _ComponentCounts[typeof(TComponent)] -= 1;

            Debug.Assert(_ComponentCounts[typeof(TComponent)] >= 0,
                $"{nameof(EntityManager)} component count for '{typeof(TComponent).Name}' is in an invalid state.");
        }

        public nint GetComponentCount(Type type) => _ComponentCounts.TryGetValue(type, out nint count) ? count : 0;

        public nint GetComponentCount<TComponent>() where TComponent : Component =>
            _ComponentCounts.TryGetValue(typeof(TComponent), out nint count) ? count : 0;

        #endregion


        #region IDisposable

        public void Dispose()
        {
            foreach (Entity entity in _Entities)
            {
                entity.Dispose();
            }

            _Entities.Dispose();
        }

        #endregion
    }
}
