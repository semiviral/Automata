using System;
using System.Collections.Generic;
using System.Threading;

namespace Automata.Engine.Collections
{
    public class ConcurrentPalette<T> : Palette<T> where T : IEquatable<T>
    {
        private readonly ReaderWriterLockSlim _AccessLock;

        public override T this[int index]
        {
            get
            {
                _AccessLock.EnterReadLock();
                T value = base[index];
                _AccessLock.ExitReadLock();

                return value;
            }
            set
            {
                _AccessLock.EnterWriteLock();
                base[index] = value;
                _AccessLock.ExitWriteLock();
            }
        }

        public ConcurrentPalette(int length, T defaultItem) : base(length, defaultItem) => _AccessLock = new ReaderWriterLockSlim();
        public ConcurrentPalette(int length, IReadOnlyCollection<T> lookupTable) : base(length, lookupTable) => _AccessLock = new ReaderWriterLockSlim();
    }
}
