using System.Buffers;
using Automata.Engine.Memory;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class BufferAllocator : BufferObject
    {
        private const BufferStorageMask _STORAGE_MASK = BufferStorageMask.DynamicStorageBit
                                                        | BufferStorageMask.MapPersistentBit
                                                        | BufferStorageMask.MapCoherentBit
                                                        | BufferStorageMask.MapWriteBit;

        private readonly NativeMemoryPool _NativeMemoryPool;

        public int AllocatedBufferCount => _NativeMemoryPool.RentedBlocks;

        public unsafe BufferAllocator(GL gl, nuint size) : base(gl)
        {
            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, size, (void*)null!, (uint)_STORAGE_MASK);
            _NativeMemoryPool = new NativeMemoryPool((byte*)GL.MapNamedBuffer(Handle, BufferAccessARB.WriteOnly), size);
        }

        public IMemoryOwner<T> Rent<T>(int size) where T : unmanaged => _NativeMemoryPool.Rent<T>(size);

        public override void Dispose()
        {
            GL.UnmapNamedBuffer(Handle);
            base.Dispose();
        }
    }
}
