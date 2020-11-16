using System;
using System.Buffers;
using Automata.Engine.Components;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class DrawIndirectAllocation : Component, IDisposable
    {
        public IMemoryOwner<byte> MemoryOwner { get; }
        public IDrawElementsIndirectCommandOwner CommandOwner { get; }

        public DrawIndirectAllocation(IMemoryOwner<byte> memoryOwner, IDrawElementsIndirectCommandOwner commandOwner)
        {
            MemoryOwner = memoryOwner;
            CommandOwner = commandOwner;
        }

        public void Dispose()
        {
            MemoryOwner?.Dispose();
            CommandOwner?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
