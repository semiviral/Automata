using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public record VertexAttribute<TPrimitive> : IVertexAttribute where TPrimitive : unmanaged
    {
        public uint Index { get; }
        public int Dimensions { get; }
        public uint Offset { get; }
        public uint BindingIndex { get; }
        public bool Normalized { get; }
        public unsafe uint Stride => (uint)(Dimensions * sizeof(TPrimitive));

        public VertexAttribute(uint index, uint dimensions, uint offset, uint bindingIndex, bool normalized = false)
        {
            Index = index;
            Dimensions = (int)dimensions;
            Offset = offset;
            BindingIndex = bindingIndex;
            Normalized = normalized;
        }

        public void CommitFormat(GL gl, uint vao)
        {
            if (typeof(TPrimitive) == typeof(int))
            {
                gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.Int, Offset);
            }
            else if (typeof(TPrimitive) == typeof(uint))
            {
                gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.UnsignedInt, Offset);
            }
            else if (typeof(TPrimitive) == typeof(short))
            {
                gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.Short, Offset);
            }
            else if (typeof(TPrimitive) == typeof(ushort))
            {
                gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.UnsignedShort, Offset);
            }
            else if (typeof(TPrimitive) == typeof(sbyte))
            {
                gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.Byte, Offset);
            }
            else if (typeof(TPrimitive) == typeof(byte))
            {
                gl.VertexArrayAttribIFormat(vao, Index, Dimensions, VertexAttribIType.UnsignedByte, Offset);
            }
            else if (typeof(TPrimitive) == typeof(float))
            {
                gl.VertexArrayAttribFormat(vao, Index, Dimensions, VertexAttribType.Float, Normalized, Offset);
            }
            else if (typeof(TPrimitive) == typeof(double))
            {
                gl.VertexArrayAttribLFormat(vao, Index, Dimensions, VertexAttribLType.Double, Offset);
            }
            else
            {
                throw new NotSupportedException($"{nameof(TPrimitive)} is of unsupported type '{typeof(TPrimitive)}'. Must be a valid OpenGL primitive.");
            }
        }

        public void CommitFormatDirect(GL gl)
        {
            if (typeof(TPrimitive) == typeof(int))
            {
                gl.VertexAttribIFormat(Index, Dimensions, VertexAttribIType.Int, Offset);
            }
            else if (typeof(TPrimitive) == typeof(uint))
            {
                gl.VertexAttribIFormat(Index, Dimensions, VertexAttribIType.UnsignedInt, Offset);
            }
            else if (typeof(TPrimitive) == typeof(short))
            {
                gl.VertexAttribIFormat(Index, Dimensions, VertexAttribIType.Short, Offset);
            }
            else if (typeof(TPrimitive) == typeof(ushort))
            {
                gl.VertexAttribIFormat(Index, Dimensions, VertexAttribIType.UnsignedShort, Offset);
            }
            else if (typeof(TPrimitive) == typeof(sbyte))
            {
                gl.VertexAttribIFormat(Index, Dimensions, VertexAttribIType.Byte, Offset);
            }
            else if (typeof(TPrimitive) == typeof(byte))
            {
                gl.VertexAttribIFormat(Index, Dimensions, VertexAttribIType.UnsignedByte, Offset);
            }
            else if (typeof(TPrimitive) == typeof(float))
            {
                gl.VertexAttribFormat(Index, Dimensions, VertexAttribType.Float, Normalized, Offset);
            }
            else if (typeof(TPrimitive) == typeof(double))
            {
                gl.VertexAttribLFormat(Index, Dimensions, VertexAttribLType.Double, Offset);
            }
            else
            {
                throw new NotSupportedException($"{nameof(TPrimitive)} is of unsupported type '{typeof(TPrimitive)}'. Must be a valid OpenGL primitive.");
            }
        }


        #region IEquatable

        public virtual bool Equals(IVertexAttribute? other) => other is not null
                                                               && (Index == other.Index)
                                                               && (Dimensions == other.Dimensions)
                                                               && (Offset == other.Offset)
                                                               && (Normalized == other.Normalized);

        public override int GetHashCode() => HashCode.Combine(Index, Dimensions, Offset, Normalized);

        #endregion
    }
}
