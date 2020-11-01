#region

using System;
using Automata.Engine.Rendering.OpenGL;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class Mesh<TDataType> : IMesh where TDataType : unmanaged
    {
        private readonly uint _AttributeStride;

        public Guid ID { get; }
        public bool Visible { get; }
        public Layer Layer { get; }

        public BufferObject<TDataType> VertexesBuffer { get; }
        public BufferObject<uint> IndexesBuffer { get; }
        public VertexArrayObject<TDataType, uint> VertexArrayObject { get; }

        public uint IndexesLength => IndexesBuffer.Length;
        public uint IndexesByteLength => IndexesBuffer.ByteLength;

        public Mesh(uint attributeStride, Layer layer = Layer.Layer0)
        {
            _AttributeStride = attributeStride;

            ID = Guid.NewGuid();
            Visible = true;
            Layer = layer;
            VertexesBuffer = new BufferObject<TDataType>(GLAPI.Instance.GL, BufferTargetARB.ArrayBuffer);
            IndexesBuffer = new BufferObject<uint>(GLAPI.Instance.GL, BufferTargetARB.ElementArrayBuffer);
            VertexArrayObject = new VertexArrayObject<TDataType, uint>(GLAPI.Instance.GL, VertexesBuffer, IndexesBuffer);
        }

        public unsafe void ModifyVertexAttributes<TAttributePrimitive>(uint index, int offset) where TAttributePrimitive : unmanaged
        {
            int dimensions = sizeof(TDataType) / sizeof(TAttributePrimitive);

            if (typeof(TAttributePrimitive) == typeof(int))
            {
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Int, _AttributeStride, offset);
            }
            else if (typeof(TAttributePrimitive) == typeof(uint))
            {
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedInt, _AttributeStride, offset);
            }
            else if (typeof(TAttributePrimitive) == typeof(short))
            {
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Short, _AttributeStride, offset);
            }
            else if (typeof(TAttributePrimitive) == typeof(ushort))
            {
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedShort, _AttributeStride, offset);
            }
            else if (typeof(TAttributePrimitive) == typeof(sbyte))
            {
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.Byte, _AttributeStride, offset);
            }
            else if (typeof(TAttributePrimitive) == typeof(byte))
            {
                VertexArrayObject.VertexAttributeIPointer(index, dimensions, VertexAttribPointerType.UnsignedByte, _AttributeStride, offset);
            }
            else if (typeof(TAttributePrimitive) == typeof(float))
            {
                VertexArrayObject.VertexAttributePointer(index, dimensions, VertexAttribPointerType.Float, _AttributeStride, offset);
            }
            else if (typeof(TAttributePrimitive) == typeof(double))
            {
                VertexArrayObject.VertexAttributeLPointer(index, dimensions, VertexAttribPointerType.Double, _AttributeStride, offset);
            }
        }

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
