using System;

namespace Automata.Engine
{
    public class RingIncrementer
    {
        private readonly nuint _Max;

        public nuint Current { get; private set; }

        public RingIncrementer(nuint max) => _Max = max;

        public void Increment() => Current = NextRing();
        public nuint NextRing() => (Current + 1u) % _Max;
    }

    public class Ring<T>
    {
        private readonly RingIncrementer _RingIncrementer;
        private readonly T[] _InternalArray;

        public T Current { get; private set; }
        public T Next { get; private set; }

        public Ring(nuint size, Func<T> objectFactory)
        {
            _RingIncrementer = new RingIncrementer(size);
            _InternalArray = new T[size];

            for (int index = 0; index < _InternalArray.Length; index++)
            {
                _InternalArray[index] = objectFactory.Invoke();
            }

            Current = _InternalArray[_RingIncrementer.Current];
            Next = _InternalArray[_RingIncrementer.NextRing()];
        }

        public void Increment()
        {
            _RingIncrementer.Increment();

            Current = _InternalArray[_RingIncrementer.Current];
            Next = _InternalArray[_RingIncrementer.NextRing()];
        }
    }
}
