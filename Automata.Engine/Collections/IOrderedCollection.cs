using System;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public interface IOrderedCollection<T> : IReadOnlyCollection<T> where T : notnull
    {
        public T this[Type type] { get; }

        public void AddFirst(T item);
        public void AddLast(T item);
        public bool AddBefore<TBefore>(T item);
        public bool AddAfter<TAfter>(T item);
        public void Remove<TItem>();
        public bool Contains<TItem>();
        public void Clear();
    }
}
