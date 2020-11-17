using System.Buffers;
using Automata.Engine.Memory;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class BufferAllocator : BufferObject
    {
        private const BufferStorageMask _STORAGE_MASK = BufferStorageMask.MapWriteBit | BufferStorageMask.MapPersistentBit | BufferStorageMask.MapCoherentBit;

        private readonly NativeMemoryPool _NativeMemoryPool;

        public int RentedBufferCount => _NativeMemoryPool.RentedBlocks;

        public unsafe BufferAllocator(GL gl, nuint size) : base(gl)
        {
            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, size, (void*)null!, (uint)_STORAGE_MASK);
            _NativeMemoryPool = new NativeMemoryPool((byte*)GL.MapNamedBufferRange(Handle, 0, (uint)size, (uint)_STORAGE_MASK), size);
        }

        public IMemoryOwner<T> Rent<T>(int size, nuint alignment, out nuint index, bool clear = false) where T : unmanaged =>
            _NativeMemoryPool.Rent<T>(size, alignment, out index, clear);


        #region IDisposable

        protected override void DisposeInternal()
        {
            GL.UnmapNamedBuffer(Handle);
            GL.DeleteBuffer(Handle);
        }

        #endregion
    }
}
