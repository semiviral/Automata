using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Automata.Engine.Collections
{
    public class OrderedList<T> : IOrderedCollection<T> where T : notnull
    {
        private readonly List<T> _InternalList;

        public OrderedList() => _InternalList = new List<T>();

        public T this[Type type] => _InternalList.First(item => type.IsInstanceOfType(item));
        public int Count => _InternalList.Count;

        public void AddFirst(T item) => _InternalList.Insert(0, item);
        public void AddLast(T item) => _InternalList.Add(item);

        public bool AddBefore<TBefore>(T item)
        {
            for (int index = 0; index < _InternalList.Count; index++)
            {
                if (_InternalList[index] is not TBefore)
                {
                    continue;
                }

                _InternalList.Insert(index, item);
                return true;
            }

            return false;
        }

        public bool AddAfter<TAfter>(T item)
        {
            for (int index = 0; index < _InternalList.Count; index++)
            {
                if (_InternalList[index] is not TAfter)
                {
                    continue;
                }

                _InternalList.Insert(index + 1, item);
                return true;
            }

            return false;
        }

        public void Remove<TItem>() => _InternalList.RemoveAll(item => item.GetType().IsInstanceOfType(typeof(TItem)));
        public bool Contains<TItem>() => _InternalList.Any(item => item.GetType().IsAssignableFrom(typeof(TItem)));
        public void Clear() => _InternalList.Clear();

        public IEnumerator<T> GetEnumerator() => _InternalList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
