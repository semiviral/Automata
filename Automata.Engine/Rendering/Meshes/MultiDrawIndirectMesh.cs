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
        private readonly VertexArrayObject<byte> _VertexArrayObject;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; }

        public MultiDrawIndirectMesh(GL gl, uint commandAllocatorSize, uint dataAllocatorSize)
        {
            _GL = gl;
            _CommandAllocator = new BufferAllocator(gl, commandAllocatorSize);
            _DataAllocator = new BufferAllocator(gl, dataAllocatorSize);
            _VertexArrayObject = new VertexArrayObject<byte>(gl, _DataAllocator, 6 * sizeof(int), _DataAllocator);
        }

        public IDrawElementsIndirectCommandOwner RentCommand() => new DrawElementsIndirectCommandOwner(_CommandAllocator.Rent<DrawElementsIndirectCommand>(1u));
        public IMemoryOwner<T> Rent<T>(uint size) where T : unmanaged => _DataAllocator.Rent<T>(size);

        public unsafe void Draw()
        {
            _VertexArrayObject.Bind();
            _CommandAllocator.Bind(BufferTargetARB.DrawIndirectBuffer);

            _GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, (void*)null!, (uint)_CommandAllocator.AllocatedBuffers, 0u);
        }

        public void Dispose()
        {
            _CommandAllocator.Dispose();
            _DataAllocator.Dispose();
            _VertexArrayObject.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
