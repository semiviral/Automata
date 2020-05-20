#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
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
        }

        public void Dispose()
        {
            _GL.DeleteVertexArray(_Handle);
        }

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offset)
        {
            _GL.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType), (void*)(offset * sizeof(TVertexType)));
            _GL.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            _GL.BindVertexArray(_Handle);
        }
    }
}
