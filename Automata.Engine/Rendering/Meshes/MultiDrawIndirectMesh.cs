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

            _VertexArrayObject.BindVertexBuffer(0u, _VertexAllocator);
            _VertexArrayObject.BindVertexBuffer(1u, _ModelBuffer, 0, 1u);

            // _GL.VertexArrayVertexBuffer(_VertexArrayObject.Handle, 0u, _VertexAllocator.Handle, 0, 8u);
            // _GL.VertexArrayVertexBuffer(_VertexArrayObject.Handle, 1u, _CommandBuffer.Handle, 0, (uint)sizeof(DrawElementsIndirectCommand));
            // _GL.VertexArrayVertexBuffer(_VertexArrayObject.Handle, 2u, _ModelBuffer.Handle, 0, (uint)sizeof(Matrix4x4));
            // _GL.VertexArrayElementBuffer(_VertexArrayObject.Handle, _IndexAllocator.Handle);
            //
            // _GL.EnableVertexArrayAttrib(_VertexArrayObject.Handle, 0u);
            // _GL.EnableVertexArrayAttrib(_VertexArrayObject.Handle, 1u);
            // _GL.EnableVertexArrayAttrib(_VertexArrayObject.Handle, 2u);
            // _GL.EnableVertexArrayAttrib(_VertexArrayObject.Handle, 3u);
            // _GL.EnableVertexArrayAttrib(_VertexArrayObject.Handle, 4u);
            // _GL.EnableVertexArrayAttrib(_VertexArrayObject.Handle, 5u);
            // _GL.EnableVertexArrayAttrib(_VertexArrayObject.Handle, 6u);
            //
            // _GL.VertexArrayAttribIFormat(_VertexArrayObject.Handle, 0u, 1, VertexAttribIType.Int, 0u);
            // _GL.VertexArrayAttribIFormat(_VertexArrayObject.Handle, 1u, 1, VertexAttribIType.Int, 4u);
            // _GL.VertexArrayAttribIFormat(_VertexArrayObject.Handle, 2u, 1, VertexAttribIType.UnsignedInt, (uint)Marshal.OffsetOf<DrawElementsIndirectCommand>(nameof(DrawElementsIndirectCommand.BaseInstance)));
            // _GL.VertexArrayAttribFormat(_VertexArrayObject.Handle, 3u + 0u, 4, VertexAttribType.Float, false, 0u);
            // _GL.VertexArrayAttribFormat(_VertexArrayObject.Handle, 3u + 1u, 4, VertexAttribType.Float, false, 16u);
            // _GL.VertexArrayAttribFormat(_VertexArrayObject.Handle, 3u + 2u, 4, VertexAttribType.Float, false, 32u);
            // _GL.VertexArrayAttribFormat(_VertexArrayObject.Handle, 3u + 3u, 4, VertexAttribType.Float, false, 48u);
            //
            // _GL.VertexArrayAttribBinding(_VertexArrayObject.Handle, 0u, 0u);
            // _GL.VertexArrayAttribBinding(_VertexArrayObject.Handle, 1u, 0u);
            // _GL.VertexArrayAttribBinding(_VertexArrayObject.Handle, 2u, 1u);
            // _GL.VertexArrayAttribBinding(_VertexArrayObject.Handle, 3u, 2u);
            // _GL.VertexArrayAttribBinding(_VertexArrayObject.Handle, 4u, 2u);
            // _GL.VertexArrayAttribBinding(_VertexArrayObject.Handle, 5u, 2u);
            // _GL.VertexArrayAttribBinding(_VertexArrayObject.Handle, 6u, 2u);
            //
            // // todo binding divisor should be apart of VertexBufferObjectBinding
            // _GL.VertexArrayBindingDivisor(_VertexArrayObject.Handle, 2u, 1u);

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
            _CommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);

#if DEBUG
            // void VerifyVertexBufferBinding(uint index, BufferObject buffer)
            // {
            //     _GL.GetInteger(, index, out int actual);
            //     Debug.Assert((uint)actual == buffer.Handle, $"VertexBindingBuffer index {index} is not set to the correct buffer.");
            // }
            //
            // VerifyVertexBufferBinding(0u, _VertexAllocator);
            // VerifyVertexBufferBinding(1u, _ModelBuffer);
#endif

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
            _ModelBuffer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
