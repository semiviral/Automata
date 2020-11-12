using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class TransparentList<T> : IList<T>
    {
        private const int _DEFAULT_SIZE = 8;

        private readonly bool _Pooled;

        private T[] _InternalArray;
        private Memory<T> _InternalMemory;

        public int Count { get; private set; }

        public bool IsReadOnly => false;
        public Memory<T> Segment => _InternalMemory.Slice(0, Count);

        public T this[int index]
        {
            get
            {
                if (index >= _InternalArray.Length) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else return _InternalArray[index];
            }
            set
            {
                if (index >= _InternalArray.Length) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else _InternalArray[index] = value;
            }
        }

        public TransparentList(bool pooled = false) : this(_DEFAULT_SIZE, pooled) { }

        public TransparentList(int capacity, bool pooled = false)
        {
            _Pooled = pooled;
            _InternalArray = pooled ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];
            _InternalMemory = _InternalArray;
        }

        public bool Contains(T item) => (Count != 0) && (IndexOf(item) != -1);

        public void Add(T item)
        {
            if (Count < _InternalArray.Length)
            {
                _InternalArray[Count] = item;
                Count += 1;
            }
            else
            {
                EnsureCapacityOrResize(Count + 1);
                _InternalArray[Count] = item;
                Count += 1;
            }
        }

        public void Insert(int index, T item)
        {
            if (index > Count) throw new ArgumentOutOfRangeException(nameof(index), "Must be non-negative and less than the size of the collection.");
            else if (index == Count) EnsureCapacityOrResize(Count + 1);
            else if (index < Count) Array.Copy(_InternalArray, index, _InternalArray, index + 1, Count - index);

            _InternalArray[index] = item;
            Count += 1;
        }

        private void EnsureCapacityOrResize(int minimumCapacity)
        {
            if (_InternalArray.Length >= minimumCapacity) return;

            int newCapacity = _InternalArray.Length == 0 ? _DEFAULT_SIZE : _InternalArray.Length * 2;

            if (newCapacity < minimumCapacity) newCapacity = minimumCapacity;

            T[] newArray = new T[newCapacity];
            Array.Copy(_InternalArray, newArray, _InternalArray.Length);
            _InternalArray = newArray;
            _InternalMemory = _InternalArray;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);

            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index >= Count) throw new ArgumentOutOfRangeException(nameof(index), "Must be non-negative and less than the size of the collection.");

            Count -= 1;

            // copies all elements from after index to the index itself, overwriting it
            if (index < Count) Array.Copy(_InternalArray, index + 1, _InternalArray, index, Count - index);

            _InternalArray[Count] = default!;
        }

        public void Clear()
        {
            if (Count == 0) return;

            Array.Clear(_InternalArray, 0, Count);
            Count = 0;
        }

        public void CopyTo(T[] array, int arrayIndex) => Array.Copy(_InternalArray, 0, array, arrayIndex, Count);
        public int IndexOf(T item) => Array.IndexOf(_InternalArray, item, 0, Count);

        public IEnumerator<T> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return _InternalArray[index];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        ~TransparentList()
        {
            if (_Pooled) ArrayPool<T>.Shared.Return(_InternalArray);
        }
    }
}
