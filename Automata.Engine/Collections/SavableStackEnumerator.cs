using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public struct SavableStackEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] _Saved;
        private readonly Stack<T> _Stack;

        private uint _SavedIndex;
        private T? _Current;

        public T Current => _Current!;
        object IEnumerator.Current => Current!;

        public SavableStackEnumerator(Stack<T> stack)
        {
            _Saved = ArrayPool<T>.Shared.Rent(stack.Count);
            _SavedIndex = 0u;
            _Stack = stack;
            _Current = default!;
        }

        public bool MoveNext()
        {
            if (_Stack.TryPop(out T? result))
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

        public void Reset() => throw new NotImplementedException();

        public void Dispose()
        {
            for (uint index = 0; index < _SavedIndex; index++)
            {
                _Stack.Push(_Saved[index]);
            }

            ArrayPool<T>.Shared.Return(_Saved);
        }
    }
}
