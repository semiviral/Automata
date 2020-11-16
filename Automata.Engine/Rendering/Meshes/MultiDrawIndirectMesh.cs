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

            VertexArrayObject = new VertexArrayObject(gl, _DataAllocator, _DataAllocator, 0u, 0);
        }

        public IDrawElementsIndirectCommandOwner RentCommand() =>
            new DrawElementsIndirectCommandOwner(_CommandAllocator.Rent<DrawElementsIndirectCommand>(1, out _));

        public IMemoryOwner<T> RentMemory<T>(int size, out nuint index, bool clear = false) where T : unmanaged =>
            _DataAllocator.Rent<T>(size, out index, clear);

        public unsafe void Draw()
        {
            VertexArrayObject.Bind();
            _CommandAllocator.Bind(BufferTargetARB.DrawIndirectBuffer);

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, (void*)null!, (uint)_CommandAllocator.RentedBufferCount, 0u);
        }

        public void Dispose()
        {
            _CommandAllocator.Dispose();
            _DataAllocator.Dispose();
            VertexArrayObject.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
