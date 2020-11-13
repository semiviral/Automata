using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class MemoryList<T> : IList<T>, IDisposable where T : IEquatable<T>
    {
        private const int _DEFAULT_SIZE = 8;

        public static readonly MemoryList<T> Empty = new MemoryList<T>(0);

        private IMemoryOwner<T> _MemoryOwner;
        private Memory<T> _InternalMemory;

        private Span<T> Span => _InternalMemory.Span;

        public bool IsReadOnly => false;
        public bool IsEmpty => Count <= 0;
        public Memory<T> Segment => _InternalMemory.Slice(0, Count);

        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index >= Count) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else return Span[index];
            }
            set
            {
                if (index >= Count) throw new IndexOutOfRangeException("Index must be non-zero and less than the size of the collection.");
                else Span[index] = value;
            }
        }

        public MemoryList(int capacity)
        {
            _MemoryOwner = MemoryPool<T>.Shared.Rent(capacity);
            _InternalMemory = _MemoryOwner.Memory;
        }

        public bool Contains(T item) => (Count != 0) && (IndexOf(item) != -1);

        public void Add(T item)
        {
            if (Count < _InternalMemory.Length)
            {
                Span[Count] = item;
                Count += 1;
            }
            else
            {
                EnsureCapacityOrResize(Count + 1);
                Span[Count] = item;
                Count += 1;
            }
        }

        public void Insert(int index, T item)
        {
            Span<T> span = Span;

            if (index > Count) throw new ArgumentOutOfRangeException(nameof(index), "Must be non-negative and less than the size of the collection.");
            else if (index == Count) EnsureCapacityOrResize(Count + 1);

            // this copies everything from index..Count to index + 1
            else if (index < Count) span.Slice(index, Count - index).CopyTo(span.Slice(index + 1));

            span[index] = item;
            Count += 1;
        }

        private void EnsureCapacityOrResize(int minimumCapacity)
        {
            if (_InternalMemory.Length >= minimumCapacity) return;

            int newCapacity = _InternalMemory.Length == 0 ? _DEFAULT_SIZE : _InternalMemory.Length * 2;

            if (newCapacity < minimumCapacity) newCapacity = minimumCapacity;

            IMemoryOwner<T> memoryOwner = MemoryPool<T>.Shared.Rent(newCapacity);
            Memory<T> memory = memoryOwner.Memory;
            _InternalMemory.CopyTo(memory);
            _MemoryOwner.Dispose();

            _MemoryOwner = memoryOwner;
            _InternalMemory = memory;
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
            Span<T> span = Span;

            // copies all elements from after index to the index itself, overwriting it
            if (index < Count) span.Slice(index + 1).CopyTo(span.Slice(index));

            span[Count] = default!;
        }

        public void Clear()
        {
            if (Count == 0) return;

            Span.Slice(0, Count).Clear();
            Count = 0;
        }

        public void CopyTo(T[] array, int arrayIndex) => Span.CopyTo(new Span<T>(array).Slice(arrayIndex));
        public int IndexOf(T item) => Span.IndexOf(item);


        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Span[index];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion


        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing) _MemoryOwner.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
