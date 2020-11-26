using System;
using System.Diagnostics;
using System.Numerics;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public class MultiDrawIndirectMesh : IMesh
    {
        private readonly GL _GL;
        private readonly BufferObject<DrawElementsIndirectCommand> _CommandBuffer;
        private readonly BufferAllocator _BufferAllocator;
        private readonly VertexArrayObject _VertexArrayObject;
        private readonly BufferObject<Matrix4x4> _ModelBuffer;
        private readonly DrawElementsType _DrawElementsType;

        public Guid ID { get; }
        public Layer Layer { get; }
        public FenceSync? DrawSync { get; private set; }

        public bool Visible => _CommandBuffer.DataLength > 0u;

        public MultiDrawIndirectMesh(GL gl, nuint allocatorSize, DrawElementsType drawElementsType, Layer layers = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Layer = layers;

            _GL = gl;
            _CommandBuffer = new BufferObject<DrawElementsIndirectCommand>(gl);
            _BufferAllocator = new BufferAllocator(gl, allocatorSize);
            _VertexArrayObject = new VertexArrayObject(gl);
            _ModelBuffer = new BufferObject<Matrix4x4>(gl, 2_500_000, BufferStorageMask.MapWriteBit);

            _VertexArrayObject.AllocateVertexBufferBinding(0u, _BufferAllocator);
            _VertexArrayObject.AllocateVertexBufferBinding(1u, _ModelBuffer, 0, 1u);
            _DrawElementsType = drawElementsType;
        }

        public void ValidateAllocatorBlocks() { _BufferAllocator.ValidateBlocks(); }


        #region State

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] attributes) =>
            _VertexArrayObject.AllocateVertexAttributes(replace, attributes);

        public void FinalizeVertexArrayObject() => _VertexArrayObject.Finalize(_BufferAllocator);

        public void AllocateDrawCommands(ReadOnlySpan<DrawElementsIndirectCommand> commands) => _CommandBuffer.SetData(commands, BufferDraw.StaticDraw);
        public void AllocateModelsData(ReadOnlySpan<Matrix4x4> models) => _ModelBuffer.SubData(0, models);

        #endregion


        #region Renting

        public BufferArrayMemory<T> RentBufferArrayMemory<T>(nuint alignment, ReadOnlySpan<T> data) where T : unmanaged =>
            new BufferArrayMemory<T>(_BufferAllocator, alignment, data);

        #endregion


        #region IMesh

        public unsafe void Draw()
        {
            _VertexArrayObject.Bind();
            _CommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);

#if DEBUG
            void VerifyVertexBufferBindingImpl(uint index, OpenGLObject buffer)
            {
                _GL.GetInteger(GLEnum.VertexBindingBuffer, index, out int actual);
                Debug.Assert((uint)actual == buffer.Handle, $"VertexBindingBuffer index {index} is not set to the correct buffer.");
            }

            VerifyVertexBufferBindingImpl(0u, _BufferAllocator);
            VerifyVertexBufferBindingImpl(1u, _ModelBuffer);
#endif

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, _DrawElementsType, (void*)null!, (uint)_CommandBuffer.DataLength, 0u);

            DrawSync?.Dispose();
            DrawSync = new FenceSync(_GL);
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
            _CommandBuffer.Dispose();
            _BufferAllocator.Dispose();
            _VertexArrayObject.Dispose();
            _ModelBuffer.Dispose();
            DrawSync?.Dispose();
        }

        ~MultiDrawIndirectMesh() => CleanupNativeResources();

        #endregion
    }
}
