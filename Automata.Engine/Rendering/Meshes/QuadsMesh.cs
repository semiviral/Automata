using System;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public abstract class QuadsMesh<TIndex, TVertex> : IMesh
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private readonly GL _GL;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; set; }
        public BufferObject BufferObject { get; }
        public VertexArrayObject VertexArrayObject { get; }
        public uint IndexesCount { get; set; }

        public QuadsMesh(GL gl, Layer layer = Layer.Layer0)
        {
            _GL = gl;

            ID = Guid.NewGuid();
            Visible = true;
            Layer = layer;
            BufferObject = new BufferObject<Quad<TIndex, TVertex>>(gl);
            VertexArrayObject = new VertexArrayObject(gl);
        }

        public abstract void Draw();

        public void Dispose()
        {
            BufferObject.Dispose();
            VertexArrayObject.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
