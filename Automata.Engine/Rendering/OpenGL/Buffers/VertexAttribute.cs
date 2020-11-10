using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public interface IVertexAttribute
    {
        public uint Index { get; }

        public void Commit(GL gl, uint vao);
    }

    public class VertexAttribute<TComponent> : IVertexAttribute, IEquatable<VertexAttribute<TComponent>> where TComponent : unmanaged
    {
        private readonly int _Dimensions;
        private readonly uint _Offset;
        private readonly bool _Normalized;

        public uint Index { get; }

        public VertexAttribute(uint index, uint dimensions, uint offset, bool normalized = false) =>
            (Index, _Dimensions, _Offset, _Normalized) = (index, (int)dimensions, offset, normalized);

        public void Commit(GL gl, uint vao)
        {
            if (typeof(TComponent) == typeof(int)) gl.VertexArrayAttribIFormat(vao, Index, _Dimensions, VertexAttribIType.Int, _Offset);
            else if (typeof(TComponent) == typeof(uint)) gl.VertexArrayAttribIFormat(vao, Index, _Dimensions, VertexAttribIType.UnsignedInt, _Offset);
            else if (typeof(TComponent) == typeof(short)) gl.VertexArrayAttribIFormat(vao, Index, _Dimensions, VertexAttribIType.Short, _Offset);
            else if (typeof(TComponent) == typeof(ushort)) gl.VertexArrayAttribIFormat(vao, Index, _Dimensions, VertexAttribIType.UnsignedShort, _Offset);
            else if (typeof(TComponent) == typeof(sbyte)) gl.VertexArrayAttribIFormat(vao, Index, _Dimensions, VertexAttribIType.Byte, _Offset);
            else if (typeof(TComponent) == typeof(byte)) gl.VertexArrayAttribIFormat(vao, Index, _Dimensions, VertexAttribIType.UnsignedByte, _Offset);
            else if (typeof(TComponent) == typeof(float)) gl.VertexArrayAttribFormat(vao, Index, _Dimensions, VertexAttribType.Float, _Normalized, _Offset);
            else if (typeof(TComponent) == typeof(double)) gl.VertexArrayAttribLFormat(vao, Index, _Dimensions, VertexAttribLType.Double, _Offset);
            else throw new NotSupportedException($"{nameof(TComponent)} is of unsupported type '{typeof(TComponent)}'. Must be a primitive.");
        }

        public bool Equals(VertexAttribute<TComponent>? other) =>
            other is not null
            && (_Dimensions == other._Dimensions)
            && (_Offset == other._Offset)
            && (_Normalized == other._Normalized)
            && (Index == other.Index);

        public override bool Equals(object? obj) => obj is VertexAttribute<TComponent> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_Dimensions, _Offset, _Normalized, Index);

        public static bool operator ==(VertexAttribute<TComponent>? left, VertexAttribute<TComponent>? right) => Equals(left, right);
        public static bool operator !=(VertexAttribute<TComponent>? left, VertexAttribute<TComponent>? right) => !Equals(left, right);
    }
}
