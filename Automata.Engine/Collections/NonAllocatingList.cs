using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class NonAllocatingList<T> : IList<T>, IDisposable where T : IEquatable<T>
    {
        private const int _DEFAULT_CAPACITY = 1;

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
                if (index >= Count) ThrowHelper.ThrowIndexOutOfRangeException();
                return _InternalArray[index];
            }
            set
            {
                if (index >= Count) ThrowHelper.ThrowIndexOutOfRangeException();
                _InternalArray[index] = value;
            }
        }

        public T this[uint index]
        {
            get
            {
                if (index >= Count) ThrowHelper.ThrowIndexOutOfRangeException();
                return _InternalArray[index];
            }
            set
            {
                if (index >= Count) ThrowHelper.ThrowIndexOutOfRangeException();
                _InternalArray[index] = value;
            }
        }

        public NonAllocatingList() : this(_DEFAULT_CAPACITY) { }
        public NonAllocatingList(int minimumCapacity) => _InternalArray = ArrayPool<T>.Shared.Rent(minimumCapacity);

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

            int idealCapacity = _InternalArray.Length is 0 ? _DEFAULT_CAPACITY : _InternalArray.Length * 2;
            int newCapacity = Math.Max(minimumCapacity, idealCapacity);

            T[] newArray = ArrayPool<T>.Shared.Rent(newCapacity);
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

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<T>
        {
            private readonly NonAllocatingList<T> _List;
            private uint _Index;
            private T? _Current;

            public T Current => _Current!;

            object? IEnumerator.Current
            {
                get
                {
                    if ((_Index == 0u) || (_Index >= (uint)_List.Count)) ThrowHelper.ThrowInvalidOperationException("Enumerable has not been enumerated.");

                    return _Current;
                }
            }

            internal Enumerator(NonAllocatingList<T> list)
            {
                _List = list;
                _Index = 0u;
                _Current = default;
            }

            public bool MoveNext()
            {
                if (_Index >= (uint)_List.Count) return false;

                _Current = _List._InternalArray[_Index];
                _Index += 1u;
                return true;
            }

            void IEnumerator.Reset()
            {
                _Index = 0u;
                _Current = default;
            }


            #region IDisposable

            public void Dispose() { }

            #endregion
        }

        #endregion


        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!disposing || _Disposed) return;

            Count = 0;
            ArrayPool<T>.Shared.Return(_InternalArray);
            _InternalArray = default!;
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
