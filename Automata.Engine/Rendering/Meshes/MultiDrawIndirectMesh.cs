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
            _IndexAllocator = new BufferAllocator(gl, indexAllocatorSize);
            _VertexAllocator = new BufferAllocator(gl, vertexAllocatorSize);
            _VertexArrayObject = new VertexArrayObject(gl);
            _CommandBuffer = new BufferObject<DrawElementsIndirectCommand>(gl);
            _ModelBuffer = new BufferObject<Matrix4x4>(gl);
            _BufferSync = new FenceSync(gl);

            _VertexArrayObject.BindVertexBuffer(0u, _VertexAllocator, 0);
            _VertexArrayObject.BindVertexBuffer(1u, _CommandBuffer, 0);
            _VertexArrayObject.BindVertexBuffer(2u, _ModelBuffer, 0);

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

        public void AllocateModelsData(Span<Matrix4x4> models) => _ModelBuffer.SetData(models, BufferDraw.StaticDraw);

        public unsafe void Draw()
        {
            _BufferSync.BusyWaitCPU();

            _VertexArrayObject.Bind();
            _CommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);
            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, _DrawElementsType, (void*)null!, DrawCommandCount, 0u);

            _BufferSync.Regenerate();
        }

        public void WaitForBufferSync() => _BufferSync.BusyWaitCPU();

        #region Renting

        public BufferArrayMemory RentIndexBufferArrayMemory(nuint alignment, nuint length) => new BufferArrayMemory(_IndexAllocator, alignment, length);
        public BufferArrayMemory RentVertexBufferArrayMemory(nuint alignment, nuint length) => new BufferArrayMemory(_VertexAllocator, alignment, length);

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
