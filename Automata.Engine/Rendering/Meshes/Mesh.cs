#region

using System;
using Automata.Engine.Rendering.OpenGL;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class Mesh<TDataType> : IMesh where TDataType : unmanaged
    {
        public BufferObject<TDataType> VertexesBuffer { get; }
        public BufferObject<uint> IndexesBuffer { get; }
        public VertexArrayObject<TDataType, uint> VertexArrayObject { get; }

        public Mesh()
        {
            ID = Guid.NewGuid();
            Visible = true;
            VertexesBuffer = new BufferObject<TDataType>(GLAPI.Instance.GL, BufferTargetARB.ArrayBuffer);
            IndexesBuffer = new BufferObject<uint>(GLAPI.Instance.GL, BufferTargetARB.ElementArrayBuffer);
            VertexArrayObject = new VertexArrayObject<TDataType, uint>(GLAPI.Instance.GL, VertexesBuffer, IndexesBuffer);
        }

        public unsafe void ModifyVertexAttributes<TAttributePrimitive>(uint index, uint stride, int offset) where TAttributePrimitive : unmanaged
        {
            int dimensions = sizeof(TDataType) / sizeof(TAttributePrimitive);

            if (typeof(TAttributePrimitive) == typeof(int))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Int, stride, offset);
            else if (typeof(TAttributePrimitive) == typeof(uint))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedInt, stride, offset);
            else if (typeof(TAttributePrimitive) == typeof(short))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Short, stride, offset);
            else if (typeof(TAttributePrimitive) == typeof(ushort))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedShort, stride, offset);
            else if (typeof(TAttributePrimitive) == typeof(sbyte))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Byte, stride, offset);
            else if (typeof(TAttributePrimitive) == typeof(byte))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedByte, stride, offset);
            else if (typeof(TAttributePrimitive) == typeof(float))
                VertexArrayObject.VertexAttributePointer(index, dimensions, VertexAttribPointerType.Float, stride, offset);
            else if (typeof(TAttributePrimitive) == typeof(double))
                VertexArrayObject.VertexAttributeLPointer(index, dimensions, VertexAttribPointerType.Double, stride, offset);
        }

        public Guid ID { get; }
        public bool Visible { get; }

        public uint IndexesLength => IndexesBuffer.Length;
        public uint IndexesByteLength => IndexesBuffer.ByteLength;

        public void Bind() => VertexArrayObject.Bind();
        public void Unbind() => VertexArrayObject.Unbind();

        public void Dispose()
        {
            VertexesBuffer.Dispose();
            IndexesBuffer.Dispose();
            VertexArrayObject.Dispose();
        }
    }
}
