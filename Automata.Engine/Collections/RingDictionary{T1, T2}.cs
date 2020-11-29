using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class RingDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Ring _Ring;
        private readonly Dictionary<TKey, TValue>[] _Dictionaries;

        public Dictionary<TKey, TValue> Current { get; private set; }
        public Dictionary<TKey, TValue> Next { get; private set; }

        public RingDictionary(nuint size)
        {
            _Ring = new Ring(size);
            _Dictionaries = new Dictionary<TKey, TValue>[size];

            for (int index = 0; index < _Dictionaries.Length; index++)
            {
                _Dictionaries[index] = new Dictionary<TKey, TValue>();
            }

            Current = _Dictionaries[_Ring.Current];
            Next = _Dictionaries[_Ring.NextRing()];
        }

        public void Increment()
        {
            _Ring.Increment();

            Current = _Dictionaries[_Ring.Current];
            Next = _Dictionaries[_Ring.NextRing()];
        }
    }
}
