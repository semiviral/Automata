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
        public bool Visible { get; }
        public VertexArrayObject VertexArrayObject { get; }

        public MultiDrawIndirectMesh(GL gl, uint commandAllocatorSize, uint dataAllocatorSize, Layer layers = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Layer = layers;
            Visible = true;

            _GL = gl;
            _CommandAllocator = new BufferAllocator(gl, commandAllocatorSize);
            _DataAllocator = new BufferAllocator(gl, dataAllocatorSize);


            VertexArrayObject = new VertexArrayObject(gl);
        }

        public IDrawElementsIndirectCommandOwner RentCommand() => new DrawElementsIndirectCommandOwner(_CommandAllocator.Rent<DrawElementsIndirectCommand>(1));
        public IMemoryOwner<T> Rent<T>(int size) where T : unmanaged => _DataAllocator.Rent<T>(size);

        public unsafe void Draw()
        {
            VertexArrayObject.Bind();
            _CommandAllocator.Bind(BufferTargetARB.DrawIndirectBuffer);

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, (void*)null!, (uint)_CommandAllocator.AllocatedBufferCount, 0u);
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
