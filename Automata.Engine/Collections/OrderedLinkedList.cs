#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion


namespace Automata.Engine.Collections
{
    public class OrderedLinkedList<T> : IOrderedCollection<T> where T : notnull
    {
        private readonly LinkedList<T> _LinkedList;
        private readonly Dictionary<Type, LinkedListNode<T>> _Nodes;

        public OrderedLinkedList()
        {
            _LinkedList = new LinkedList<T>();
            _Nodes = new Dictionary<Type, LinkedListNode<T>>();
        }

        public void Remove<TItem>()
        {
            _LinkedList.Remove(_Nodes[typeof(TItem)]);
            _Nodes.Remove(typeof(TItem));
        }

        public T this[Type type] => _Nodes[type].Value;

        public bool Contains<TItem>() => _Nodes.ContainsKey(typeof(TItem));

        public void AddFirst(T item) => _Nodes.Add(item.GetType(), _LinkedList.AddFirst(item));
        public void AddLast(T item) => _Nodes.Add(item.GetType(), _LinkedList.AddLast(item));

        public bool AddBefore<TBefore>(T item)
        {
            if (_Nodes.ContainsKey(typeof(TBefore)))
            {
                _Nodes.Add(item.GetType(), _LinkedList.AddBefore(_Nodes[typeof(TBefore)], item));
                return true;
            }
            else return false;
        }

        public bool AddAfter<TAfter>(T item)
        {
            if (_Nodes.ContainsKey(typeof(TAfter)))
            {
                _Nodes.Add(item.GetType(), _LinkedList.AddAfter(_Nodes[typeof(TAfter)], item));
                return true;
            }
            else return false;
        }

        public void Clear()
        {
            _LinkedList.Clear();
            _Nodes.Clear();
        }

        public int Count => _Nodes.Count;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _LinkedList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    }
}
