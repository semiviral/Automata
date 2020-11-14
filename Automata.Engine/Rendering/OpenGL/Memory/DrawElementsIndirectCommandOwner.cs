using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL.Memory
{
    public interface IDrawElementsIndirectCommandOwner : IDisposable
    {
        public DrawElementsIndirectCommand Command { get; }
    }

    internal sealed record DrawElementsIndirectCommandOwner : IDrawElementsIndirectCommandOwner
    {
        private readonly IMemoryOwner<DrawElementsIndirectCommand> _MemoryOwner;

        public DrawElementsIndirectCommand Command { get; }

        public DrawElementsIndirectCommandOwner(IMemoryOwner<DrawElementsIndirectCommand> memoryOwner)
        {
            _MemoryOwner = memoryOwner;
            Command = memoryOwner.Memory.Span[0];
        }

        public void Dispose() => _MemoryOwner.Dispose();
    }
}
