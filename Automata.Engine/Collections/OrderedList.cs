#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion


namespace Automata.Engine.Collections
{
    public class OrderedList<T> : IReadOnlyCollection<T> where T : notnull
    {
        private readonly LinkedList<T> _LinkedList;
        private readonly Dictionary<Type, LinkedListNode<T>> _Nodes;

        public T this[Type type] => _Nodes[type].Value;
        public int Count => _Nodes.Count;

        public OrderedList()
        {
            _LinkedList = new LinkedList<T>();
            _Nodes = new Dictionary<Type, LinkedListNode<T>>();
        }

        public bool Contains<TItem>() => _Nodes.ContainsKey(typeof(TItem));

        public void AddFirst(T item) => _Nodes.Add(item.GetType(), _LinkedList.AddFirst(item));
        public void AddLast(T item) => _Nodes.Add(item.GetType(), _LinkedList.AddLast(item));
        public void AddBefore<TBefore>(T item) => _Nodes.Add(item.GetType(), _LinkedList.AddBefore(_Nodes[typeof(TBefore)], item));
        public void AddAfter<TAfter>(T item) => _Nodes.Add(item.GetType(), _LinkedList.AddAfter(_Nodes[typeof(TAfter)], item));

        public bool Remove<TItem>()
        {
            _LinkedList.Remove(_Nodes[typeof(TItem)]);
            return _Nodes.Remove(typeof(TItem));
        }

        public void Clear()
        {
            _LinkedList.Clear();
            _Nodes.Clear();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _LinkedList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    }
}
