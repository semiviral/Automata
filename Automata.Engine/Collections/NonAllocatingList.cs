using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class NonAllocatingList<T> : IList<T>, IEquatable<NonAllocatingList<T>>, IDisposable where T : IEquatable<T>
    {
        private const int _DEFAULT_CAPACITY = 1;

        public static readonly NonAllocatingList<T> Empty = new NonAllocatingList<T>(0);

        private T[] _InternalArray;

        public bool Disposed { get; private set; }

        public Span<T> Segment => new Span<T>(_InternalArray, 0, Count);
        public bool IsReadOnly => Disposed;
        public bool IsEmpty => Count <= 0;

        public int Count { get; private set; }

        public T this[int index] { get => _InternalArray[(uint)index]; set => _InternalArray[(uint)index] = value; }

        public T this[uint index]
        {
            get
            {
                if (index >= Count)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                return _InternalArray[index];
            }
            set
            {
                if (index >= Count)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                _InternalArray[index] = value;
            }
        }

        public NonAllocatingList() : this(_DEFAULT_CAPACITY) { }
        public NonAllocatingList(int minimumCapacity) => _InternalArray = ArrayPool<T>.Shared.Rent(minimumCapacity);

        public void Add(T item)
        {
            if (Count >= _InternalArray.Length)
            {
                EnsureCapacityOrResize(Count + 1);
            }

            _InternalArray[Count] = item;
            Count += 1;
        }

        public void AddRange(ReadOnlySpan<T> items) => InsertRange(Count, items);

        public void Insert(int index, T item)
        {
            if (index > Count)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }
            else if (index == Count)
            {
                EnsureCapacityOrResize(Count + 1);
            }

            // this copies everything from index..Count to index + 1
            else if (index < Count)
            {
                Segment.Slice(index).CopyTo(_InternalArray.AsSpan().Slice(index + 1));
            }

            _InternalArray[index] = item;
            Count += 1;
        }

        public void InsertRange(int index, ReadOnlySpan<T> items)
        {
            if ((uint)index > (uint)Count)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            // ensure we have enough capacity to insert the items
            int endIndex = index + items.Length;
            EnsureCapacityOrResize(endIndex);

            Span<T> internalSpan = _InternalArray.AsSpan();

            if (index != Count)
            {
                // make space for the insertion, copying the elements past it
                internalSpan.Slice(index, Count).CopyTo(internalSpan.Slice(endIndex));
            }

            // copy items to range
            items.CopyTo(internalSpan.Slice(index));
            Count += items.Length;
        }

        private void EnsureCapacityOrResize(int minimumCapacity)
        {
            if (_InternalArray.Length >= minimumCapacity)
            {
                return;
            }

            int idealCapacity = _InternalArray.Length is 0 ? _DEFAULT_CAPACITY : _InternalArray.Length * 2;
            int newCapacity = Math.Max(minimumCapacity, idealCapacity);

            T[] newArray = ArrayPool<T>.Shared.Rent(newCapacity);
            Segment.CopyTo(newArray);
            _InternalArray = newArray;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);

            if (index == -1)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index >= Count)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            Count -= 1;

            // copies all elements from after index to the index itself, overwriting it
            if (index < Count)
            {
                Segment.Slice(index + 1).CopyTo(_InternalArray.AsSpan().Slice(index));
            }

            _InternalArray[Count] = default!;
        }

        public bool Contains(T item) => (Count != 0) && (IndexOf(item) != -1);
        public int IndexOf(T item) => Segment.IndexOf(item);
        public void CopyTo(T[] array, int arrayIndex) => Segment.CopyTo(new Span<T>(array, arrayIndex, array.Length - arrayIndex));

        public void Clear()
        {
            if (Count == 0)
            {
                return;
            }

            Segment.Clear();
            Count = 0;
        }


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
                    if ((_Index == 0u) || (_Index >= (uint)_List.Count))
                    {
                        ThrowHelper.ThrowInvalidOperationException("Enumerable has not been enumerated.");
                    }

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
                if (_Index >= (uint)_List.Count)
                {
                    return false;
                }

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


        #region IEquatable

        public override bool Equals(object? obj) => obj is NonAllocatingList<T> other && Equals(other);

        public bool Equals(NonAllocatingList<T>? other)
        {
            if (other is null || (other.Count != Count))
            {
                return false;
            }
            else
            {
                ReadOnlySpan<T> thisSegment = Segment;
                ReadOnlySpan<T> otherSegment = other.Segment;

                for (int index = 0; index < Count; index++)
                {
                    if (!thisSegment[index].Equals(otherSegment[index]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion


        #region IDisposable

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            DisposeInternal();

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        private void DisposeInternal()
        {
            Count = 0;
            ArrayPool<T>.Shared.Return(_InternalArray);
            _InternalArray = default!;
        }

        #endregion
    }
}
