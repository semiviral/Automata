#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

#endregion


namespace Automata.Engine.Collections
{
    public class FixedConcurrentQueue<T> : IProducerConsumerCollection<T>, IReadOnlyCollection<T>
    {
        private readonly ConcurrentQueue<T> _InternalQueue;
        private int _Count;

        public bool IsEmpty => _InternalQueue.IsEmpty;

        public int MaximumSize { get; }

        public FixedConcurrentQueue(int maximumSize)
        {
            _InternalQueue = new ConcurrentQueue<T>();
            MaximumSize = maximumSize;
        }

        public FixedConcurrentQueue(IEnumerable<T> collection)
        {
            _InternalQueue = new ConcurrentQueue<T>(collection);
            MaximumSize = _Count = _InternalQueue.Count;
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot =>
            throw new NotSupportedException("The SyncRoot property may not be used for the synchronization of concurrent collections.");

        public int Count => _Count;

        public IEnumerator<T> GetEnumerator() => _InternalQueue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T[] ToArray() => _InternalQueue.ToArray();

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Enqueue(item);

            return true;
        }

        bool IProducerConsumerCollection<T>.TryTake([MaybeNullWhen(false)] out T item) => TryDequeue(out item);

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is T[] arrayAs) _InternalQueue.CopyTo(arrayAs, index);
            else throw new ArgumentNullException(nameof(array));
        }

        public void CopyTo(T[] array, int index) { _InternalQueue.CopyTo(array, index); }

        public void Enqueue(T item)
        {
            _InternalQueue.Enqueue(item);

            if (Count == MaximumSize) _InternalQueue.TryDequeue(out T _);
            else Interlocked.Increment(ref _Count);
        }

        private bool TryDequeue([MaybeNullWhen(false)] out T item) => _InternalQueue.TryDequeue(out item);

        public bool TryPeek([MaybeNullWhen(false)] out T item) => _InternalQueue.TryPeek(out item);
    }
}
