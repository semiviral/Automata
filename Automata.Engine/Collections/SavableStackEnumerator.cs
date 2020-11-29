using System;
using System.Buffers;
using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public ref struct SavableStackEnumerator<T>
    {
        private readonly T[] _Cache;
        private readonly Stack<T> _Stack;

        private uint _SavedIndex;
        private T? _Current;

        public T Current => _Current!;

        public SavableStackEnumerator(Stack<T> stack)
        {
            _Cache = ArrayPool<T>.Shared.Rent(stack.Count);
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
            _Cache[_SavedIndex] = Current;
            _SavedIndex += 1u;
        }

        public void Reset() => throw new NotImplementedException();

        public void Dispose()
        {
            for (uint index = 0; index < _SavedIndex; index++)
            {
                _Stack.Push(_Cache[index]);
            }

            ArrayPool<T>.Shared.Return(_Cache);
        }
    }
}
