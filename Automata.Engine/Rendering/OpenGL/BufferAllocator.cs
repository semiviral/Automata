using System;
using System.Buffers;
using Automata.Engine.Memory;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class BufferAllocator : OpenGLObject, IDisposable
    {
        private const BufferStorageMask _STORAGE_MASK = BufferStorageMask.DynamicStorageBit
                                                        | BufferStorageMask.MapPersistentBit
                                                        | BufferStorageMask.MapCoherentBit
                                                        | BufferStorageMask.MapWriteBit;

        private readonly NativeMemoryPool _NativeMemoryPool;

        public int AllocatedBuffers => _NativeMemoryPool.RentedBlocks;

        public unsafe BufferAllocator(GL gl, uint size) : base(gl)
        {
            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, size, (void*)null!, (uint)_STORAGE_MASK);
            byte* pointer = (byte*)GL.MapNamedBuffer(Handle, GLEnum.WriteOnly);
            _NativeMemoryPool = new NativeMemoryPool(pointer, size);
        }

        public IMemoryOwner<T> Rent<T>(uint size) where T : unmanaged => _NativeMemoryPool.Rent<T>(size);

        public void Bind(BufferTargetARB target) => GL.BindBuffer(target, Handle);

        public void Dispose()
        {
            GL.UnmapNamedBuffer(Handle);
            GL.DeleteBuffer(Handle);
            GC.SuppressFinalize(this);
        }
    }
}
