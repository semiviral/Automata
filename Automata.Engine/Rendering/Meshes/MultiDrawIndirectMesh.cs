using System;
using System.Diagnostics;
using System.Numerics;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public class MultiDrawIndirectMesh<TIndex, TVertex> : IMesh
        where TIndex : unmanaged
        where TVertex : unmanaged
    {
        private readonly GL _GL;
        private readonly BufferAllocator _IndexAllocator;
        private readonly BufferAllocator _VertexAllocator;
        private readonly VertexArrayObject _VertexArrayObject;
        private readonly BufferObject<DrawElementsIndirectCommand> _CommandBuffer;
        private readonly BufferObject<Matrix4x4> _ModelBuffer;
        private readonly FenceSync _BufferSync;
        private readonly DrawElementsType _DrawElementsType;

        public Guid ID { get; }
        public Layer Layer { get; }
        public uint DrawCommandCount { get; private set; }
        public bool Visible => DrawCommandCount > 0u;

        public MultiDrawIndirectMesh(GL gl, uint indexAllocatorSize, uint vertexAllocatorSize, Layer layers = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Layer = layers;

            _GL = gl;
            _CommandBuffer = new BufferObject<DrawElementsIndirectCommand>(gl);
            _IndexAllocator = new BufferAllocator(gl, indexAllocatorSize);
            _VertexAllocator = new BufferAllocator(gl, vertexAllocatorSize);
            _VertexArrayObject = new VertexArrayObject(gl);
            _ModelBuffer = new BufferObject<Matrix4x4>(gl);
            _BufferSync = new FenceSync(gl);

            _VertexArrayObject.BindVertexBuffer(0u, _VertexAllocator, 0);
            _VertexArrayObject.BindVertexBuffer(1u, _CommandBuffer, 0);
            _VertexArrayObject.BindVertexBuffer(2u, _ModelBuffer, 0);

            if (typeof(TIndex) == typeof(byte))
            {
                _DrawElementsType = DrawElementsType.UnsignedByte;
            }
            else if (typeof(TIndex) == typeof(ushort))
            {
                _DrawElementsType = DrawElementsType.UnsignedShort;
            }
            else if (typeof(TIndex) == typeof(uint))
            {
                _DrawElementsType = DrawElementsType.UnsignedInt;
            }
            else
            {
                throw new NotSupportedException("Does not support specified index type.");
            }
        }

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] attributes) =>
            _VertexArrayObject.AllocateVertexAttributes(replace, attributes);

        public void FinalizeVertexArrayObject() => _VertexArrayObject.Finalize(_IndexAllocator);

        public void AllocateDrawElementsIndirectCommands(Span<DrawElementsIndirectCommand> commands)
        {
            DrawCommandCount = (uint)commands.Length;
            _CommandBuffer.SetData(commands, BufferDraw.StaticDraw);
        }

        public void AllocateModelsData(Span<Matrix4x4> models) => _ModelBuffer.SetData(models, BufferDraw.StaticDraw);
        public void WaitForBufferSync() => _BufferSync.BusyWaitCPU();


        #region IMesh

        public unsafe void Draw()
        {
            _BufferSync.BusyWaitCPU();

            _VertexArrayObject.Bind();

#if DEBUG

            // verify vertex buffer bindings

            _GL.GetInteger(GLEnum.VertexBindingBuffer, 0u, out int data);
            _GL.GetInteger(GLEnum.VertexBindingBuffer, 1u, out int commands);
            _GL.GetInteger(GLEnum.VertexBindingBuffer, 2u, out int models);

            Debug.Assert((uint)data == _VertexAllocator.Handle);
            Debug.Assert((uint)commands == _CommandBuffer.Handle);
            Debug.Assert((uint)models == _ModelBuffer.Handle);
#endif

            _CommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);
            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, _DrawElementsType, (void*)null!, DrawCommandCount, 0u);

            _BufferSync.Regenerate();
        }

        #endregion


        #region Renting

        public BufferArrayMemory<TIndex> RentIndexBufferArrayMemory(nuint alignment, ReadOnlySpan<TIndex> data) =>
            new BufferArrayMemory<TIndex>(_IndexAllocator, alignment, data);

        public BufferArrayMemory<TVertex> RentVertexBufferArrayMemory(nuint alignment, ReadOnlySpan<TVertex> data) =>
            new BufferArrayMemory<TVertex>(_VertexAllocator, alignment, data);

        #endregion


        public void Dispose()
        {
            _BufferSync.Dispose();
            _CommandBuffer.Dispose();
            _IndexAllocator.Dispose();
            _VertexAllocator.Dispose();
            _VertexArrayObject.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
