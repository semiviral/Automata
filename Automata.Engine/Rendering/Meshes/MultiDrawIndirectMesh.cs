using System;
using System.Numerics;
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
        private readonly BufferObject _CommandBuffer;
        private readonly BufferAllocator _IndexAllocator;
        private readonly BufferAllocator _VertexAllocator;
        private readonly VertexArrayObject _VertexArrayObject;
        private readonly ShaderStorageBufferObject<Matrix4x4> _Models;
        private readonly DrawElementsType _DrawElementsType;

        private nint _BufferSync;

        public Guid ID { get; }
        public Layer Layer { get; }
        public uint DrawCommandCount { get; private set; }
        public bool Visible => DrawCommandCount > 0u;

        public MultiDrawIndirectMesh(GL gl, uint indexAllocatorSize, uint vertexAllocatorSize, Layer layers = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Layer = layers;

            _GL = gl;
            _CommandBuffer = new BufferObject(gl);
            _IndexAllocator = new BufferAllocator(gl, indexAllocatorSize);
            _VertexAllocator = new BufferAllocator(gl, vertexAllocatorSize);
            _VertexArrayObject = new VertexArrayObject(gl);
            _Models = new ShaderStorageBufferObject<Matrix4x4>(gl, 5u);

            _VertexArrayObject.BindVertexBuffer(0u, _VertexAllocator, 0);
            _VertexArrayObject.BindVertexBuffer(1u, _CommandBuffer, 0);

            if (typeof(TIndex) == typeof(byte)) _DrawElementsType = DrawElementsType.UnsignedByte;
            else if (typeof(TIndex) == typeof(ushort)) _DrawElementsType = DrawElementsType.UnsignedShort;
            else if (typeof(TIndex) == typeof(uint)) _DrawElementsType = DrawElementsType.UnsignedInt;
            else throw new NotSupportedException("Does not support specified index type.");
        }

        public void FinalizeVertexArrayObject() => _VertexArrayObject.Finalize(_IndexAllocator);

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] attributes) =>
            _VertexArrayObject.AllocateVertexAttributes(replace, attributes);

        public void AllocateDrawElementsIndirectCommands(Span<DrawElementsIndirectCommand> commands)
        {
            DrawCommandCount = (uint)commands.Length;
            _CommandBuffer.SetData(commands, BufferDraw.StaticDraw);
        }

        public void SetSSBOModelsData(Span<Matrix4x4> models) => _Models.SetData(models);

        public unsafe void Draw()
        {
            WaitForBufferFreeSync();

            _Models.Bind();
            _VertexArrayObject.Bind();
            _CommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, _DrawElementsType, (void*)null!, DrawCommandCount, 0u);

            RegenerateBufferSync();
        }


        #region Renting

        public BufferArrayMemory RentIndexBufferArrayMemory(nuint alignment, nuint length) => new BufferArrayMemory(_IndexAllocator, alignment, length);
        public BufferArrayMemory RentVertexBufferArrayMemory(nuint alignment, nuint length) => new BufferArrayMemory(_VertexAllocator, alignment, length);

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
