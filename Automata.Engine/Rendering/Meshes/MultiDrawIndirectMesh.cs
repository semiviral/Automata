using System;
using System.Buffers;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Memory;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Meshes
{

    public class MultiDrawIndirectMesh
    {
        private readonly BufferAllocator _CommandAllocator;
        private readonly BufferAllocator _DataAllocator;
        private readonly VertexArrayObject<byte> _VertexArrayObject;

        public MultiDrawIndirectMesh(GL gl, uint commandAllocatorSize, uint dataAllocatorSize)
        {
            const uint one_kb = 1000u;
            const uint one_mb = 1000u * one_kb;
            const uint one_gb = 1000u * one_mb;

            _CommandAllocator = new BufferAllocator(gl, commandAllocatorSize);
            _DataAllocator = new BufferAllocator(gl, dataAllocatorSize);
            _VertexArrayObject = new VertexArrayObject<byte>(gl, _DataAllocator, 6 * sizeof(int), _DataAllocator);
        }

        public IDrawElementsIndirectCommandOwner RentCommand() => new DrawElementsIndirectCommandOwner(_CommandAllocator.Rent<DrawElementsIndirectCommand>(1u));
        public IMemoryOwner<T> Rent<T>(uint size) where T : unmanaged => _DataAllocator.Rent<T>(size);

        public void Bind()
        {
            _CommandAllocator.Bind(BufferTargetARB.DrawIndirectBuffer);
            _VertexArrayObject.Bind();
        }
    }
}
