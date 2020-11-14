using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL.Memory
{
    internal sealed unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        private readonly T* _Pointer;
        private readonly uint _Length;
        private readonly Memory<T> _Memory;

        public NativeMemoryManager(T* pointer, uint length)
        {
            _Pointer = pointer;
            _Length = length;
            _Memory = CreateMemory((int)length);
        }

        public Memory<T> Slice(uint start) => _Memory.Slice((int)start);
        public Memory<T> Slice(uint start, uint length) => _Memory.Slice((int)start, (int)length);

        #region MemoryManager

        public override Span<T> GetSpan() => new Span<T>(_Pointer, (int)_Length);

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
