using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL
{
    public interface IDrawElementsIndirectCommandOwner : IDisposable
    {
        public ref DrawElementsIndirectCommand Command { get; }
    }

    internal sealed record DrawElementsIndirectCommandOwner : IDrawElementsIndirectCommandOwner
    {
        private readonly IMemoryOwner<DrawElementsIndirectCommand> _MemoryOwner;

        public ref DrawElementsIndirectCommand Command => ref _MemoryOwner.Memory.Span[0];

        public DrawElementsIndirectCommandOwner(IMemoryOwner<DrawElementsIndirectCommand> memoryOwner) => _MemoryOwner = memoryOwner;

        public void Dispose() => _MemoryOwner.Dispose();
    }
}
