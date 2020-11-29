using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public struct SavableQueueEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] _Saved;
        private readonly Queue<T> _Queue;

        private uint _SavedIndex;
        private T? _Current;

        public T Current => _Current!;
        object IEnumerator.Current => Current!;

        public SavableQueueEnumerator(Queue<T> queue)
        {
            _Saved = ArrayPool<T>.Shared.Rent(queue.Count);
            _SavedIndex = 0u;
            _Queue = queue;
            _Current = default!;
        }

        public bool MoveNext()
        {
            if (_Queue.TryDequeue(out T? result))
            {
                _Current = result;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SaveCurrent()
        {
            _Saved[_SavedIndex] = Current;
            _SavedIndex += 1u;
        }

        public void Reset() => throw new NotSupportedException();

        public void Dispose()
        {
            for (uint index = 0; index < _SavedIndex; index++)
            {
                _Queue.Enqueue(_Saved[index]);
            }

            ArrayPool<T>.Shared.Return(_Saved);
        }
    }
}
