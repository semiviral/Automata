using System;
using System.Buffers;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public class MultiDrawIndirectMesh<TIndex, TVertex> : IMesh
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private readonly GL _GL;
        private readonly MultiDrawElementsIndirectCommandBuffer _CommandBuffer;
        private readonly BufferAllocator _IndexAllocator;
        private readonly BufferAllocator _VertexAllocator;
        private readonly VertexArrayObject _VertexArrayObject;
        private readonly DrawElementsType _DrawElementsType;

        private nint _BufferSync;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible => _CommandBuffer.Count > 0;

        public MultiDrawIndirectMesh(GL gl, uint indexAllocatorSize, uint vertexAllocatorSize, Layer layers = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Layer = layers;

            _GL = gl;
            _CommandBuffer = new MultiDrawElementsIndirectCommandBuffer(gl);
            _IndexAllocator = new BufferAllocator(gl, indexAllocatorSize);
            _VertexAllocator = new BufferAllocator(gl, vertexAllocatorSize);
            _VertexArrayObject = new VertexArrayObject(gl);

            if (typeof(TIndex) == typeof(byte)) _DrawElementsType = DrawElementsType.UnsignedByte;
            else if (typeof(TIndex) == typeof(ushort)) _DrawElementsType = DrawElementsType.UnsignedShort;
            else if (typeof(TIndex) == typeof(uint)) _DrawElementsType = DrawElementsType.UnsignedInt;
            else throw new NotSupportedException("Does not support specified index type.");
        }

        public void FinalizeVertexArrayObject(int vertexOffset) => _VertexArrayObject.Finalize(_VertexAllocator, _IndexAllocator, vertexOffset);

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] attributes) =>
            _VertexArrayObject.AllocateVertexAttributes(replace, attributes);

        public void AllocateDrawElementsIndirectCommands(Span<DrawElementsIndirectCommand> commands) => _CommandBuffer.AllocateCommands(commands);

        public unsafe void Draw()
        {
            WaitForBufferFreeSync();

            _CommandBuffer.Bind();
            _VertexArrayObject.Bind();

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, _DrawElementsType, (void*)null!, _CommandBuffer.Count, 0u);

            RegenerateBufferSync();
        }


        #region Renting

        public IMemoryOwner<TIndex> RentIndexMemory(int size, nuint alignment, out nuint index, bool clear = false) =>
            _IndexAllocator.Rent<TIndex>(size, alignment, out index, clear);

        public IMemoryOwner<TVertex> RentVertexMemory(int size, nuint alignment, out nuint index, bool clear = false) =>
            _VertexAllocator.Rent<TVertex>(size, alignment, out index, clear);

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
            _CommandBuffer.Dispose();
            _IndexAllocator.Dispose();
            _VertexAllocator.Dispose();
            _VertexArrayObject.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
