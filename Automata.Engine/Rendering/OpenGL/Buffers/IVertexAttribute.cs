using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public interface IVertexAttribute : IEquatable<IVertexAttribute>
    {
        public uint Index { get; }
        public int Dimensions { get; }
        public uint Offset { get; }
        public uint Divisor { get; }
        public bool Normalized { get; }
        public uint Stride { get; }

        public void Commit(GL gl, uint vao);
    }
}
