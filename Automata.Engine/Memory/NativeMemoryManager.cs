using System;
using System.Buffers;

namespace Automata.Engine.Memory
{
    internal sealed unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        private readonly T* _Pointer;
        private readonly int _Length;

        public NativeMemoryManager(T* pointer, int length)
        {
            _Pointer = pointer;
            _Length = length;
        }


        #region MemoryManager

        public override Span<T> GetSpan() => new Span<T>(_Pointer, _Length);

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if ((elementIndex < 0) || (elementIndex >= _Length)) throw new ArgumentOutOfRangeException(nameof(elementIndex));

            return new MemoryHandle(_Pointer + elementIndex);
        }

        public override void Unpin() => throw new InvalidOperationException("Functionality not supported.");

        #endregion


        #region IDisposable

        protected override void Dispose(bool disposing) => throw new NotImplementedException();

        #endregion
    }
}
