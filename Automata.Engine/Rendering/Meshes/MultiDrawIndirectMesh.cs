using System;
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
        private readonly FenceSync _BufferSync;
        private readonly BufferObject<DrawElementsIndirectCommand> _CommandBuffer;
        private readonly DrawElementsType _DrawElementsType;
        private readonly GL _GL;
        private readonly BufferAllocator _IndexAllocator;
        private readonly BufferObject<Matrix4x4> _ModelBuffer;
        private readonly BufferAllocator _VertexAllocator;
        private readonly VertexArrayObject _VertexArrayObject;
        public uint DrawCommandCount { get; private set; }

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

            _VertexArrayObject.AllocateVertexBufferBinding(0u, _VertexAllocator);
            _VertexArrayObject.AllocateVertexBufferBinding(1u, _ModelBuffer, 0, 1u);

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

        public void AllocateDrawCommands(Span<DrawElementsIndirectCommand> commands)
        {
            DrawCommandCount = (uint)commands.Length;
            _CommandBuffer.SetData(commands, BufferDraw.StaticDraw);
        }

        public void AllocateModelsData(Span<Matrix4x4> models) => _ModelBuffer.SetData(models, BufferDraw.StaticDraw);
        public void WaitForBufferSync() => _BufferSync.BusyWaitCPU();

        public void ValidateAllocatorBlocks()
        {
            _IndexAllocator.ValidateBlocks();
            _VertexAllocator.ValidateBlocks();
        }

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible => DrawCommandCount > 0u;


        #region IMesh

        public unsafe void Draw()
        {
            _BufferSync.BusyWaitCPU();

            _GL.VertexArrayVertexBuffer(_VertexArrayObject.Handle, 1u, _ModelBuffer.Handle, 0, (uint)sizeof(Matrix4x4));
            _VertexArrayObject.Bind();
            _CommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);

#if DEBUG
            void VerifyVertexBufferBindingImpl(uint index, BufferObject buffer)
            {
                _GL.GetInteger(GLEnum.VertexBindingBuffer, index, out int actual);
                Debug.Assert((uint)actual == buffer.Handle, $"VertexBindingBuffer index {index} is not set to the correct buffer.");
            }

            VerifyVertexBufferBindingImpl(0u, _VertexAllocator);
            VerifyVertexBufferBindingImpl(1u, _ModelBuffer);
#endif

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, _DrawElementsType, (void*)null!, DrawCommandCount, 0u);

            _BufferSync.Regenerate();
        }

        #endregion


        #region IDisposable

        public void Dispose()
        {
            CleanupNativeResources();
            GC.SuppressFinalize(this);
        }

        private void CleanupNativeResources()
        {
            _BufferSync.Dispose();
            _CommandBuffer.Dispose();
            _IndexAllocator.Dispose();
            _VertexAllocator.Dispose();
            _VertexArrayObject.Dispose();
            _ModelBuffer.Dispose();
        }

        ~MultiDrawIndirectMesh() => CleanupNativeResources();

        #endregion


        #region Renting

        public BufferArrayMemory<TIndex> RentIndexBufferArrayMemory(nuint alignment, ReadOnlySpan<TIndex> data) =>
            new BufferArrayMemory<TIndex>(_IndexAllocator, alignment, data);

        public BufferArrayMemory<TVertex> RentVertexBufferArrayMemory(nuint alignment, ReadOnlySpan<TVertex> data) =>
            new BufferArrayMemory<TVertex>(_VertexAllocator, alignment, data);

        #endregion
    }
}
