using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class ShaderStorageBufferObject<T> : OpenGLObject where T : unmanaged
    {
        public uint BindingIndex { get; }
        public uint Size { get; private set; }

        public ShaderStorageBufferObject(GL gl, uint bindingIndex) : base(gl)
        {
            Handle = GL.CreateBuffer();
            BindingIndex = bindingIndex;
        }

        public unsafe void SetData(Span<T> data)
        {
            GL.NamedBufferData(Handle, (uint)(data.Length * sizeof(T)), data, VertexBufferObjectUsage.StaticRead);
            Size = (uint)data.Length;
        }


        #region Binding

        public void Bind() => GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, BindingIndex, Handle);

        #endregion


        #region IDisposable

        protected override void CleanupNativeResources() => GL.DeleteBuffer(Handle);

        #endregion
    }
}
