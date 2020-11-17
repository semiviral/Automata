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
        private readonly BufferAllocator _CommandAllocator;
        private readonly BufferAllocator _DataAllocator;
        private readonly VertexArrayObject _VertexArrayObject;

        private nint _BufferSync;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; set; }

        public MultiDrawIndirectMesh(GL gl, uint commandAllocatorSize, uint dataAllocatorSize, Layer layers = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Layer = layers;

            _GL = gl;
            _CommandAllocator = new BufferAllocator(gl, commandAllocatorSize);
            _DataAllocator = new BufferAllocator(gl, dataAllocatorSize);
            _VertexArrayObject = new VertexArrayObject(gl);
        }

        public void FinalizeVertexArrayObject(int vertexOffset) => _VertexArrayObject.Finalize(_DataAllocator, _DataAllocator, vertexOffset);

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] attributes) =>
            _VertexArrayObject.AllocateVertexAttributes(replace, attributes);

        public unsafe void Draw()
        {
            WaitForBufferFreeSync();

            _VertexArrayObject.Bind();
            _CommandAllocator.Bind(BufferTargetARB.DrawIndirectBuffer);

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, (void*)null!, (uint)_CommandAllocator.RentedBufferCount, 0u);

            RegenerateBufferSync();
        }


        #region Renting

        public unsafe IDrawElementsIndirectCommandOwner RentCommand(out nuint index) =>
            new DrawElementsIndirectCommandOwner(_CommandAllocator.Rent<DrawElementsIndirectCommand>(1, (uint)sizeof(DrawElementsIndirectCommand), out index));

        public IMemoryOwner<T> RentMemory<T>(int size, nuint alignment, out nuint index, bool clear = false) where T : unmanaged =>
            _DataAllocator.Rent<T>(size, alignment, out index, clear);

        #endregion


        #region BufferSync

        public void WaitForBufferFreeSync()
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
            _VertexArrayObject.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
