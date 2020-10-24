#region

using System;
using Automata.Engine.Rendering.OpenGL;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class Mesh<TDataType> : IMesh where TDataType : unmanaged
    {
        public Guid ID { get; }
        public bool Visible { get; }
        public BufferObject<TDataType> VertexesBuffer { get; }
        public BufferObject<uint> IndexesBuffer { get; }
        public VertexArrayObject<TDataType, uint> VertexArrayObject { get; }

        public uint IndexesLength => IndexesBuffer.Length;

        public Mesh()
        {
            ID = Guid.NewGuid();
            Visible = true;
            VertexesBuffer = new BufferObject<TDataType>(GLAPI.Instance.GL, BufferTargetARB.ArrayBuffer);
            IndexesBuffer = new BufferObject<uint>(GLAPI.Instance.GL, BufferTargetARB.ElementArrayBuffer);
            VertexArrayObject = new VertexArrayObject<TDataType, uint>(GLAPI.Instance.GL, VertexesBuffer, IndexesBuffer);
        }

        public unsafe void ModifyVertexAttributes<TAttributePrimitive>(uint index, int offset) where TAttributePrimitive : unmanaged
        {
            int dimensions = sizeof(TDataType) / sizeof(TAttributePrimitive);

            if (typeof(TAttributePrimitive) == typeof(int)) VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Int, offset);
            else if (typeof(TAttributePrimitive) == typeof(uint))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedInt, offset);
            else if (typeof(TAttributePrimitive) == typeof(short))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Short, offset);
            else if (typeof(TAttributePrimitive) == typeof(ushort))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedShort, offset);
            else if (typeof(TAttributePrimitive) == typeof(sbyte))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Byte, offset);
            else if (typeof(TAttributePrimitive) == typeof(byte))
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedByte, offset);
            else if (typeof(TAttributePrimitive) == typeof(float))
                VertexArrayObject.VertexAttributePointer(index, dimensions, VertexAttribPointerType.Float, offset);
            else if (typeof(TAttributePrimitive) == typeof(double))
                VertexArrayObject.VertexAttributeLPointer(index, dimensions, VertexAttribPointerType.Double, offset);
        }

        public void BindVertexArrayObject() => VertexArrayObject.Bind();

        public void Dispose()
        {
            VertexesBuffer.Dispose();
            IndexesBuffer.Dispose();
            VertexArrayObject.Dispose();
        }
    }
}
