#region

using System;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private readonly GL _GL;
        private readonly uint _Handle;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {
            _GL = gl;
            _Handle = _GL.GenVertexArray();

            Bind();
            vbo.Bind();
            ebo.Bind();
            Unbind();
        }

        public unsafe void VertexAttributePointer(uint attributeIndex, int dimensions, VertexAttribPointerType type, uint stride, int offset)
        {
            Bind();

            _GL.EnableVertexAttribArray(attributeIndex);
            _GL.VertexAttribPointer(attributeIndex, dimensions, type, false, stride, (void*)(offset * sizeof(TVertexType)));

            Unbind();
        }

        public unsafe void VertexAttributeIPointer(uint attributeIndex, int dimensions, VertexAttribPointerType type, uint stride, int offset)
        {
            Bind();

            _GL.EnableVertexAttribArray(attributeIndex);
            _GL.VertexAttribIPointer(attributeIndex, dimensions, type, stride, (void*)(offset * sizeof(TVertexType)));

            Unbind();
        }

        public unsafe void VertexAttributeLPointer(uint attributeIndex, int dimensions, VertexAttribPointerType type, uint stride, int offset)
        {
            Bind();

            _GL.EnableVertexAttribArray(attributeIndex);
            _GL.VertexAttribLPointer(attributeIndex, dimensions, type, stride, (void*)(offset * sizeof(TVertexType)));

            Unbind();
        }

        public void Bind() => _GL.BindVertexArray(_Handle);
        public void Unbind() => _GL.BindVertexArray(0);

        public void Dispose() => _GL.DeleteVertexArray(_Handle);
    }
}
