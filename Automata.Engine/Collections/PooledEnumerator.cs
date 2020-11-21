using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public struct PooledEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] _PooledArray;

        private uint _Index;
        private uint _Length;
        private T? _Current;

        public T Current => _Current!;

        object? IEnumerator.Current
        {
            get
            {
                if ((_Index == 0u) || (_Index >= (uint)_Length))
                {
                    ThrowHelper.ThrowInvalidOperationException("Enumerable has not been enumerated.");
                }

                return Current;
            }
        }

        internal PooledEnumerator(T[] pooledArray, uint length)
        {
            _PooledArray = pooledArray;
            _Index = 0u;
            _Length = length;
            _Current = default!;
        }

        public bool MoveNext()
        {
            if (_Index >= (uint)_Length)
            {
                return false;
            }

            _Current = _PooledArray[_Index];
            _Index += 1u;
            return true;
        }

        void IEnumerator.Reset()
        {
            _Index = 0u;
            _Current = default!;
        }

        public PooledEnumerator<T> GetEnumerator() => this;

        #region IDisposable

        public void Dispose() => ArrayPool<T>.Shared.Return(_PooledArray);

        #endregion
    }
}
