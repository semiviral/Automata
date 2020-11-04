using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public interface IVertexAttribute
    {
        public uint Index { get; }
        public uint Offset { get; }
        public bool Normalized { get; }
        public int Size { get; }

        public void Commit(GL gl, uint vaoHandle);
    }

    public readonly struct VertexAttribute<TAttribute, TComponent> : IVertexAttribute where TAttribute : unmanaged where TComponent : unmanaged
    {
        public uint Index { get; }
        public uint Offset { get; }
        public bool Normalized { get; }
        public unsafe int Size => sizeof(TAttribute);

        public VertexAttribute(uint index, uint offset, bool normalized) => (Index, Offset, Normalized) = (index, offset, normalized);

        public unsafe void Commit(GL gl, uint vaoHandle)
        {
            if (typeof(TComponent) == typeof(int)) gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.Int, Normalized, Offset);
            else if (typeof(TComponent) == typeof(uint))
                gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.UnsignedInt, Normalized, Offset);
            else if (typeof(TComponent) == typeof(short))
                gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.Short, Normalized, Offset);
            else if (typeof(TComponent) == typeof(ushort))
                gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.UnsignedShort, Normalized, Offset);
            else if (typeof(TComponent) == typeof(sbyte))
                gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.Byte, Normalized, Offset);
            else if (typeof(TComponent) == typeof(byte))
                gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.UnsignedByte, Normalized, Offset);
            else if (typeof(TComponent) == typeof(float))
                gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.Float, Normalized, Offset);
            else if (typeof(TComponent) == typeof(double))
                gl.VertexArrayAttribFormat(vaoHandle, Index, sizeof(TAttribute), VertexAttribType.Double, Normalized, Offset);
            else throw new NotSupportedException($"{nameof(TComponent)} is of unsupported type '{typeof(TComponent)}'. Must be a primitive.");
        }
    }
}
