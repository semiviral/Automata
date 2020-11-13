using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class MemoryList<T> : IList<T>
    {
        private const int _DEFAULT_SIZE = 8;

        private readonly bool _Pooled;

        private T[] _InternalArray = null!;
        private Memory<T> _InternalMemory = null!;

        private T[] InternalArray
        {
            get => _InternalArray;
            set
            {
                _InternalArray = value;
                _InternalMemory = _InternalArray;
            }
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;
        public Memory<T> Segment => _InternalMemory.Slice(0, Count);

        public T this[int index]
        {
            get
            {
                if (index >= InternalArray.Length) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else return InternalArray[index];
            }
            set
            {
                if (index >= InternalArray.Length) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else InternalArray[index] = value;
            }
        }

        public MemoryList(bool pooled = false) : this(_DEFAULT_SIZE, pooled) { }

        public MemoryList(int capacity, bool pooled = false)
        {
            _Pooled = pooled;
            InternalArray = RetrieveNewArray(capacity);
        }

        private T[] RetrieveNewArray(int capacity) => _Pooled ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];

        public bool Contains(T item) => (Count != 0) && (IndexOf(item) != -1);

        public void Add(T item)
        {
            if (Count < InternalArray.Length)
            {
                InternalArray[Count] = item;
                Count += 1;
            }
            else
            {
                EnsureCapacityOrResize(Count + 1);
                InternalArray[Count] = item;
                Count += 1;
            }
        }

        public void Insert(int index, T item)
        {
            if (index > Count) throw new ArgumentOutOfRangeException(nameof(index), "Must be non-negative and less than the size of the collection.");
            else if (index == Count) EnsureCapacityOrResize(Count + 1);
            else if (index < Count) Array.Copy(InternalArray, index, InternalArray, index + 1, Count - index);

            InternalArray[index] = item;
            Count += 1;
        }

        private void EnsureCapacityOrResize(int minimumCapacity)
        {
            if (InternalArray.Length >= minimumCapacity) return;

            int newCapacity = _InternalArray.Length == 0 ? _DEFAULT_SIZE : InternalArray.Length * 2;

            if (newCapacity < minimumCapacity) newCapacity = minimumCapacity;

            T[] newArray = _Pooled ? ;
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

        ~MemoryList()
        {
            if (_Pooled) ArrayPool<T>.Shared.Return(_InternalArray);
        }
    }
}
