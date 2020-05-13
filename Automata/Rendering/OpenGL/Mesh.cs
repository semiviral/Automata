#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Singletons;
using Silk.NET.OpenGL;

// ReSharper disable InconsistentNaming

#endregion

namespace Automata.Rendering.OpenGL
{
    public class Mesh<TVertexType> where TVertexType : unmanaged
    {
        private GL _GL { get; }
        private VertexBuffer<TVertexType> VerticesBuffer { get; }
        private BufferObject<uint> TrianglesBuffer { get; }

        public IEnumerable<TVertexType> Vertices { get; set; } = Enumerable.Empty<TVertexType>();
        public IEnumerable<uint> Triangles { get; set; } = Enumerable.Empty<uint>();

        public Mesh()
        {
            if (GLAPI.Instance == null)
            {
                throw new InvalidOperationException($"Singleton '{GLAPI.Instance}' has not been instantiated.");
            }

            _GL = GLAPI.Instance.GL;

            VerticesBuffer = new VertexBuffer<TVertexType>(_GL);
            TrianglesBuffer = new BufferObject<uint>(_GL, BufferTargetARB.ElementArrayBuffer);
        }

        public void Flush()
        {
            if ((Vertices == null) || !Vertices.Any())
            {
                throw new ArgumentNullException(string.Format(ExceptionFormats.ArgumentNullException, nameof(Vertices)));
            }
            else if ((Triangles == null) || !Triangles.Any())
            {
                throw new ArgumentNullException(string.Format(ExceptionFormats.ArgumentNullException, nameof(Triangles)));
            }

            VerticesBuffer.SetBufferData(Vertices.ToArray());
            TrianglesBuffer.SetBufferData(Triangles.ToArray());
        }
    }
}
