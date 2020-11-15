using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class NonAllocatingList<T> : IList<T>, IDisposable where T : IEquatable<T>
    {
        private const int _DEFAULT_CAPACITY = 2;

        public static readonly NonAllocatingList<T> Empty = new NonAllocatingList<T>(0);

        private T[] _InternalArray;
        private bool _Disposed;

        public bool IsReadOnly => false;
        public bool IsEmpty => Count <= 0;
        public Span<T> Segment => new Span<T>(_InternalArray, 0, Count);

        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index >= Count) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else return _InternalArray[index];
            }
            set
            {
                if (index >= Count) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else _InternalArray[index] = value;
            }
        }

        public NonAllocatingList() : this(_DEFAULT_CAPACITY) { }
        public NonAllocatingList(int capacity) => _InternalArray = ArrayPool<T>.Shared.Rent(capacity);

        public bool Contains(T item) => (Count != 0) && (IndexOf(item) != -1);

        public void Add(T item)
        {
            if (Count >= _InternalArray.Length) EnsureCapacityOrResize(Count + 1);

            _InternalArray[Count] = item;
            Count += 1;
        }

        public void Insert(int index, T item)
        {
            if (index > Count) throw new ArgumentOutOfRangeException(nameof(index), "Must be non-negative and less than the size of the collection.");
            else if (index == Count) EnsureCapacityOrResize(Count + 1);

            // this copies everything from index..Count to index + 1
            else if (index < Count) Array.Copy(_InternalArray, index, _InternalArray, index + 1, Count - index);

            _InternalArray[index] = item;
            Count += 1;
        }

        private void EnsureCapacityOrResize(int minimumCapacity)
        {
            if (_InternalArray.Length >= minimumCapacity) return;

            int newCapacity = _InternalArray.Length == 0 ? _DEFAULT_CAPACITY : _InternalArray.Length * 2;

            if (newCapacity < minimumCapacity) newCapacity = minimumCapacity;

            T[] newArray = ArrayPool<T>.Shared.Rent(minimumCapacity);
            Array.Copy(_InternalArray, 0, newArray, 0, Count);
            _InternalArray = newArray;
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
            if (index < Count) Array.Copy(_InternalArray, index + 1, _InternalArray, index, Count - (index + 1));

            _InternalArray[Count] = default!;
        }

        public void Clear()
        {
            if (Count == 0) return;

            Array.Clear(_InternalArray, 0, Count);
            Count = 0;
        }

        public void CopyTo(T[] array, int arrayIndex) => Array.Copy(_InternalArray, 0, array, arrayIndex, Count);
        public int IndexOf(T item) => Array.IndexOf(_InternalArray, item);


        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return _InternalArray[index];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion


        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!disposing || _Disposed) return;

            ArrayPool<T>.Shared.Return(_InternalArray);
            _Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
