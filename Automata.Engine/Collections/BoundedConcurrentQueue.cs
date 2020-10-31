#region

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion


namespace Automata.Engine.Collections
{
    public class BoundedConcurrentQueue<T> : IReadOnlyCollection<T>
    {
        private readonly ConcurrentQueue<T> _ConcurrentQueue;
        public int MaximumSize { get; }

        public BoundedConcurrentQueue(int maximumSize) => (_ConcurrentQueue, MaximumSize) = (new ConcurrentQueue<T>(), maximumSize);

        public void Enqueue(T item)
        {
            _ConcurrentQueue.Enqueue(item);

            if (_ConcurrentQueue.Count > MaximumSize)
            {
                TryDequeue(out _);
            }
        }

        public bool TryDequeue([MaybeNullWhen(false)] out T item) => _ConcurrentQueue.TryDequeue(out item);

        public int Count => _ConcurrentQueue.Count;

        public IEnumerator<T> GetEnumerator() => _ConcurrentQueue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
