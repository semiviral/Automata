using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public interface IVertexAttribute : IEquatable<IVertexAttribute>
    {
        public uint Index { get; }
        public int Dimensions { get; }
        public uint Offset { get; }
        public uint BindingIndex { get; }
        public bool Normalized { get; }
        public uint Stride { get; }

        public void CommitFormat(GL gl, uint vao);
        public void CommitFormatDirect(GL gl);
    }
}
