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
        }

        public unsafe void VertexAttributePointer(uint index, int dimensions, VertexAttribPointerType type, int offset)
        {
            _GL.VertexAttribPointer(index, dimensions, type, false, (uint)dimensions * (uint)sizeof(TVertexType), (void*)(offset * sizeof(TVertexType)));
            _GL.EnableVertexAttribArray(index);
        }

        public unsafe void VertexAttributeIPointer(uint index, int dimensions, VertexAttribPointerType type, int offset)
        {
            _GL.VertexAttribIPointer(index, dimensions, type, (uint)dimensions * (uint)sizeof(TVertexType), (void*)(offset * sizeof(TVertexType)));
            _GL.EnableVertexAttribArray(index);
        }

        public unsafe void VertexAttributeLPointer(uint index, int dimensions, VertexAttribPointerType type, int offset)
        {
            _GL.VertexAttribLPointer(index, dimensions, type, (uint)dimensions * (uint)sizeof(TVertexType), (void*)(offset * sizeof(TVertexType)));
            _GL.EnableVertexAttribArray(index);
        }

        public void Bind() => _GL.BindVertexArray(_Handle);
        public void Dispose() => _GL.DeleteVertexArray(_Handle);
    }
}
