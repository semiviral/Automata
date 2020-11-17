using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public record VertexAttribute<TComponent> : IVertexAttribute where TComponent : unmanaged
    {
        public uint Index { get; }
        public int Dimensions { get; }
        public uint Offset { get; }
        public uint BindingIndex { get; }
        public uint Divisor { get; }
        public bool Normalized { get; }
        public unsafe uint Stride => (uint)(Dimensions * sizeof(TComponent));

        public VertexAttribute(uint index, uint dimensions, uint offset, uint bindingIndex = 0u, uint divisor = 0u, bool normalized = false)
        {
            Index = index;
            Dimensions = (int)dimensions;
            Offset = offset;
            BindingIndex = bindingIndex;
            Divisor = divisor;
            Normalized = normalized;
        }

        public void Commit(GL gl, uint vao)
        {
            if (typeof(TComponent) == typeof(int)) gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.Int, Offset);
            else if (typeof(TComponent) == typeof(uint)) gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.UnsignedInt, Offset);
            else if (typeof(TComponent) == typeof(short)) gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.Short, Offset);
            else if (typeof(TComponent) == typeof(ushort)) gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.UnsignedShort, Offset);
            else if (typeof(TComponent) == typeof(sbyte)) gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.Byte, Offset);
            else if (typeof(TComponent) == typeof(byte)) gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.UnsignedByte, Offset);
            else if (typeof(TComponent) == typeof(float)) gl.VertexArrayAttribFormat(vao, Index, Dimensions, VertexAttribType.Float, Normalized, Offset);
            else if (typeof(TComponent) == typeof(double)) gl.VertexArrayAttribLFormat(vao, Index, Dimensions, VertexAttribLType.Double, Offset);
            else throw new NotSupportedException($"{nameof(TComponent)} is of unsupported type '{typeof(TComponent)}'. Must be a primitive.");
        }


        #region IEquatable

        public virtual bool Equals(IVertexAttribute? other) => other is not null
                                                               && (Index == other.Index)
                                                               && (Dimensions == other.Dimensions)
                                                               && (Offset == other.Offset)
                                                               && (Divisor == other.Divisor)
                                                               && (Normalized == other.Normalized);

        public override int GetHashCode() => HashCode.Combine(Index, Dimensions, Offset, Divisor, Normalized);

        #endregion
    }
}
