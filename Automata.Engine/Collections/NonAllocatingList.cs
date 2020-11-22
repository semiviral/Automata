using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Collections
{
    public class NonAllocatingList<T> : IList<T>, IEquatable<NonAllocatingList<T>>, IDisposable where T : IEquatable<T>
    {
        private const int _DEFAULT_CAPACITY = 1;

        public static readonly NonAllocatingList<T> Empty = new NonAllocatingList<T>(0);

        private T[] _InternalArray;

        /// <summary>
        ///     Whether the <see cref="NonAllocatingList{T}" /> has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        ///     Wraps the currently addressed section (determined by <see cref="Count" />) of the internal array.
        /// </summary>
        public Span<T> Segment => new Span<T>(_InternalArray, 0, Count);

        /// <summary>
        ///     Whether the <see cref="NonAllocatingList{T}" /> is empty.
        /// </summary>
        public bool IsEmpty => Count <= 0;

        /// <summary>
        ///     The current total capacity of the internal array.
        /// </summary>
        public int Capacity => _InternalArray.Length;

        /// <summary>
        ///     Sets or gets the element at the given index.
        /// </summary>
        /// <param name="index">
        ///     Index to get or set element at.
        /// </param>
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

        public NonAllocatingList(IEnumerable<T> items)
        {
            if (items is ICollection<T> collection)
            {
                if (collection.Count is 0)
                {
                    _InternalArray = ArrayPool<T>.Shared.Rent(_DEFAULT_CAPACITY);
                }
                else
                {
                    _InternalArray = ArrayPool<T>.Shared.Rent(collection.Count);
                    collection.CopyTo(_InternalArray, 0);
                    Count = collection.Count;
                }
            }
            else
            {
                _InternalArray = ArrayPool<T>.Shared.Rent(_DEFAULT_CAPACITY);

                foreach (T item in items)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        ///     Copies the contents of the <see cref="NonAllocatingList{T}" /> to the destination <see cref="Span{T}" />.
        /// </summary>
        /// <param name="destination">
        ///     <see cref="Span{T}" /> to copy contents to.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> destination) => Segment.CopyTo(destination);

        /// <summary>
        ///     Fills the <see cref="NonAllocatingList{T}" /> with the given item.
        /// </summary>
        /// <param name="item">
        ///     Item to fill with.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T item) => Segment.Fill(item);

        /// <summary>
        ///     Clears the contents of the <see cref="NonAllocatingList{T}" />, and optionally returns its used memory (down to the default capacity).
        /// </summary>
        /// <param name="compress">
        ///     Whether to compress the capacity of the <see cref="NonAllocatingList{T}" />.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool compress = false)
        {
            if (Count is 0)
            {
                return;
            }

            Segment.Clear();
            Count = 0;

            if (compress)
            {
                ArrayPool<T>.Shared.Return(_InternalArray);
                _InternalArray = ArrayPool<T>.Shared.Rent(_DEFAULT_CAPACITY);
            }
        }

        /// <summary>
        ///     The current count of items.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Whether the <see cref="NonAllocatingList{T}" /> is read only.
        /// </summary>
        public bool IsReadOnly => Disposed;

        /// <summary>
        ///     Sets or gets the element at the given index.
        /// </summary>
        /// <param name="index">
        ///     Index to get or set element at.
        /// </param>
        public T this[int index] { get => _InternalArray[(uint)index]; set => _InternalArray[(uint)index] = value; }

        /// <summary>
        ///     Determine whether or not the <see cref="NonAllocatingList{T}" /> contains the given item.
        /// </summary>
        /// <param name="item">
        ///     Item to search for.
        /// </param>
        /// <returns>
        ///     <c>True</c> if the item is found, otherwise <c>False</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => (Count != 0) && (IndexOf(item) != -1);

        /// <summary>
        ///     Find the first occuring index of the given item.
        /// </summary>
        /// <param name="item">
        ///     Item to search for.
        /// </param>
        /// <returns>
        ///     Returns -1 if the item is not found, otherwise returns the index the item was found at.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item) => Segment.IndexOf(item);


        #region Adding / Inserting

        /// <summary>
        ///     Adds the an item to the end of the <see cref="NonAllocatingList{T}" />.
        /// </summary>
        /// <remarks>
        ///     If the <see cref="NonAllocatingList{T}" />'s capacity cannot hold value, the capacity is doubled.
        /// </remarks>
        /// <param name="item">Item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (Count >= _InternalArray.Length)
            {
                EnsureCapacityOrResize(Count + 1);
            }

            _InternalArray[Count] = item;
            Count += 1;
        }

        /// <summary>
        ///     Adds a given range of items.
        /// </summary>
        /// <remarks>
        ///     If the <see cref="NonAllocatingList{T}" />'s capacity cannot hold the items, the capacity is doubled.
        /// </remarks>
        /// <param name="items">Items to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> items) => InsertRange(Count, items);

        /// <summary>
        ///     Inserts an item at a particular index.
        /// </summary>
        /// <remarks>
        ///     If the <see cref="NonAllocatingList{T}" />'s capacity cannot hold the items, the capacity is doubled.
        /// </remarks>
        /// <param name="index">
        ///     Index to insert item at.
        /// </param>
        /// <param name="item">
        ///     Item to insert.
        /// </param>
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

        /// <summary>
        ///     Inserts a range of items, starting from a given index.
        /// </summary>
        /// <remarks>
        ///     If the <see cref="NonAllocatingList{T}" />'s capacity cannot hold the items, the capacity is doubled.
        /// </remarks>
        /// <param name="index">
        ///     Index to insert items at.
        /// </param>
        /// <param name="items">
        ///     Items to insert.
        /// </param>
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnsureCapacityOrResize(int minimumCapacity)
        {
            if (_InternalArray.Length >= minimumCapacity)
            {
                return;
            }

            int idealCapacity = _InternalArray.Length is 0 ? _DEFAULT_CAPACITY : _InternalArray.Length * 2;
            int newCapacity = Math.Max(minimumCapacity, idealCapacity);

            T[] newArray = ArrayPool<T>.Shared.Rent(newCapacity);
            CopyTo(newArray);
            _InternalArray = newArray;
        }

        #endregion


        #region Removing

        /// <summary>
        ///     Removes an item from the <see cref="NonAllocatingList{T}" />.
        /// </summary>
        /// <param name="item">
        ///     Item to remove.
        /// </param>
        /// <returns>
        ///     <c>True</c> if the item was removed, otherwise <c>False</c>.
        /// </returns>
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

        /// <summary>
        ///     Removes the item at the given index.
        /// </summary>
        /// <param name="index">
        ///     Index to remove item at.
        /// </param>
        public void RemoveAt(int index)
        {
            if (index > Count)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            // copies all elements from after index to the index itself, overwriting it
            if (index <= Count)
            {
                Segment.Slice(index + 1).CopyTo(_InternalArray.AsSpan().Slice(index));
            }
            else
            {
                _InternalArray[index] = default!;
            }

            Count -= 1;
        }

        #endregion


        #region ICollection

        /// <inheritdoc cref="CopyTo" />
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Segment.CopyTo(new Span<T>(array, arrayIndex, array.Length - arrayIndex));

        /// <inheritdoc cref="Clear" />
        void ICollection<T>.Clear()
        {
            if (Count > 0)
            {
                return;
            }

            Segment.Clear();
            Count = 0;
        }

        #endregion


        #region IEnumerable

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
        public Enumerator GetEnumerator() => new Enumerator(this);

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
                _Current = default!;
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
                _Current = default!;
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

            SafeDispose();

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        private void SafeDispose()
        {
            Count = 0;
            ArrayPool<T>.Shared.Return(_InternalArray);
            _InternalArray = default!;
        }

        #endregion
    }
}
