using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Automata.Engine.Collections
{
    public class DerivedSet<T> : IDictionary<Type, T>
    {
        private readonly Dictionary<Type, T> _InternalDictionary;

        public int Count => _InternalDictionary.Count;
        public bool IsReadOnly => (_InternalDictionary as IDictionary<Type, T>).IsReadOnly;

        public Dictionary<Type, T>.KeyCollection Types => _InternalDictionary.Keys;
        public Dictionary<Type, T>.ValueCollection Items => _InternalDictionary.Values;

        public T this[Type type] { get => _InternalDictionary[type]; set => _InternalDictionary[type] = value; }

        public DerivedSet() => _InternalDictionary = new Dictionary<Type, T>();

        public void Add<TType>(TType item) where TType : T => _InternalDictionary.Add(typeof(TType), item);

        public bool Remove<TType>() => _InternalDictionary.Remove(typeof(TType));
        public bool Remove(Type type) => _InternalDictionary.Remove(type);

        public bool Contains<TType>() => _InternalDictionary.ContainsKey(typeof(TType));
        public bool Contains(Type type) => _InternalDictionary.ContainsKey(type);

        public TType GetItem<TType>() where TType : class, T => (_InternalDictionary[typeof(TType)] as TType)!;

        public bool TryGetItem<TType>([MaybeNullWhen(false)] out TType? item) where TType : class, T
        {
            if (_InternalDictionary.TryGetValue(typeof(TType), out T? value))
            {
                item = (value as TType)!;
                return true;
            }

            item = null;
            return false;
        }

        public bool TryGetItem(Type type, [MaybeNullWhen(false)] out T? item) => _InternalDictionary.TryGetValue(type, out item);

        public void Clear() => _InternalDictionary.Clear();


        #region ICollection

        void ICollection<KeyValuePair<Type, T>>.Add(KeyValuePair<Type, T> item) =>
            (_InternalDictionary as ICollection<KeyValuePair<Type, T>>).Add(item);

        bool ICollection<KeyValuePair<Type, T>>.Contains(KeyValuePair<Type, T> item) =>
            (_InternalDictionary as ICollection<KeyValuePair<Type, T>>).Contains(item);

        bool ICollection<KeyValuePair<Type, T>>.Remove(KeyValuePair<Type, T> item) =>
            (_InternalDictionary as ICollection<KeyValuePair<Type, T>>).Remove(item);

        void ICollection<KeyValuePair<Type, T>>.CopyTo(KeyValuePair<Type, T>[] array, int arrayIndex) =>
            (_InternalDictionary as ICollection<KeyValuePair<Type, T>>).CopyTo(array, arrayIndex);

        #endregion


        #region IDictionary

        ICollection<Type> IDictionary<Type, T>.Keys => _InternalDictionary.Keys;
        ICollection<T> IDictionary<Type, T>.Values => _InternalDictionary.Values;

        void IDictionary<Type, T>.Add(Type key, T value) => _InternalDictionary.Add(key, value);
        bool IDictionary<Type, T>.ContainsKey(Type key) => _InternalDictionary.ContainsKey(key);
        bool IDictionary<Type, T>.TryGetValue(Type key, [MaybeNullWhen(false)] out T value) => _InternalDictionary.TryGetValue(key, out value);

        #endregion


        #region IEnumerable

        public Dictionary<Type, T>.Enumerator GetEnumerator() => _InternalDictionary.GetEnumerator();
        IEnumerator<KeyValuePair<Type, T>> IEnumerable<KeyValuePair<Type, T>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
