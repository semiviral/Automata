using System;
using System.Buffers;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public class MultiDrawIndirectMesh : IMesh
    {
        private readonly GL _GL;
        public readonly BufferAllocator _CommandAllocator;
        public readonly BufferAllocator _DataAllocator;

        private nint _BufferSync;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; set; }
        public VertexArrayObject VertexArrayObject { get; }

        public MultiDrawIndirectMesh(GL gl, uint commandAllocatorSize, uint dataAllocatorSize, Layer layers = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Layer = layers;
            Visible = true;

            _GL = gl;
            _CommandAllocator = new BufferAllocator(gl, commandAllocatorSize);
            _DataAllocator = new BufferAllocator(gl, dataAllocatorSize);

            VertexArrayObject = new VertexArrayObject(gl, _DataAllocator, _DataAllocator, (uint)(sizeof(int) * 2u), 0);
        }

        public unsafe IDrawElementsIndirectCommandOwner RentCommand(out nuint index) =>
            new DrawElementsIndirectCommandOwner(_CommandAllocator.Rent<DrawElementsIndirectCommand>(1, (uint)sizeof(DrawElementsIndirectCommand), out index));

        public IMemoryOwner<T> RentMemory<T>(int size, nuint alignment, out nuint index, bool clear = false) where T : unmanaged =>
            _DataAllocator.Rent<T>(size, alignment, out index, clear);

        public unsafe void Draw()
        {
            //WaitForBufferSync();

            VertexArrayObject.Bind();
            _CommandAllocator.Bind(BufferTargetARB.DrawIndirectBuffer);

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, (void*)null!, (uint)_CommandAllocator.RentedBufferCount, 0u);

            //RegenerateBufferSync();
        }


        #region BufferSync

        public void WaitForBufferSync()
        {
            if (_BufferSync is 0) return;

            while (true)
                switch ((SyncStatus)_GL.ClientWaitSync(_BufferSync, (uint)GLEnum.SyncFlushCommandsBit, 1))
                {
                    case SyncStatus.AlreadySignaled:
                    case SyncStatus.ConditionSatisfied: return;
                }
        }

        private void RegenerateBufferSync()
        {
            DisposeBufferSync();
            _BufferSync = _GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0u);
        }

        private void DisposeBufferSync()
        {
            if (_BufferSync is not 0) _GL.DeleteSync(_BufferSync);
        }

        #endregion


        public void Dispose()
        {
            DisposeBufferSync();
            _CommandAllocator.Dispose();
            _DataAllocator.Dispose();
            VertexArrayObject.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
