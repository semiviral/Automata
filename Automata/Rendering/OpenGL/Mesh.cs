#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class Mesh
    {
        private GL _GL { get; }
        private VertexBuffer VertexBuffer { get; }
        private BufferObject<uint> TrisBuffer { get; }

        public IEnumerable<Vector3> Vertices { get; set; }
        public IEnumerable<Color64> Colors { get; set; }
        public IEnumerable<uint> Triangles { get; set; }

        public Mesh()
        {
            _GL = GL.GetApi();

            VertexBuffer = new VertexBuffer(_GL);
            TrisBuffer = new BufferObject<uint>(_GL, BufferTargetARB.ElementArrayBuffer);
        }

        public void Flush()
        {
            if ((Vertices == null) || !Vertices.Any())
            {
                throw new ArgumentNullException(string.Format(ExceptionFormats.ArgumentNullException,
                    nameof(Vertices)));
            }
            else if ((Triangles == null) || !Triangles.Any())
            {
                throw new ArgumentNullException(string.Format(ExceptionFormats.ArgumentNullException,
                    nameof(Triangles)));
            }

            if ((Colors == null) || !Colors.Any())
            {
                VertexBuffer.SetBufferData(Vertices);
            }
            else
            {
                VertexBuffer.SetBufferData(Vertices, Colors);
            }

            TrisBuffer.SetBufferData(Triangles.ToArray());
        }
    }
}
