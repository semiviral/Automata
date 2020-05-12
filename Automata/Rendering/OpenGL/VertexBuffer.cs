#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class VertexBuffer : BufferObject<float>
    {
        public VertexBuffer(GL gl) : base(gl, BufferTargetARB.ArrayBuffer) { }

        public void SetBufferData(IEnumerable<Vector3> vertices)
        {
            SetBufferData(TranslateVector3Data(vertices).ToArray());
        }

        public void SetBufferData(IEnumerable<Vector3> vertices, IEnumerable<Color64> colors)
        {
            SetBufferData(TranslateVector3AndColorData(vertices, colors).ToArray());
        }

        private static IEnumerable<float> TranslateVector3Data(IEnumerable<Vector3> vertices)
        {
            if (vertices == null)
            {
                throw new NullReferenceException(nameof(vertices));
            }

            foreach (Vector3 vertex in vertices)
            {
                yield return vertex.X;
                yield return vertex.Y;
                yield return vertex.Z;
            }
        }

        private static IEnumerable<float> TranslateVector3AndColorData(IEnumerable<Vector3> vertices, IEnumerable<Color64> colors)
        {
            if (vertices == null)
            {
                throw new NullReferenceException(nameof(vertices));
            }
            else if (colors == null)
            {
                throw new NullReferenceException(nameof(colors));
            }

            using IEnumerator<Color64> colorsEnumerator = colors.GetEnumerator();

            foreach (Vector3 vertex in vertices)
            {
                if (!colorsEnumerator.MoveNext())
                {
                    throw new ArgumentException("Given enumerables must match in length.");
                }

                yield return vertex.X;
                yield return vertex.Y;
                yield return vertex.Z;

                Color64 color = colorsEnumerator.Current;
                yield return color.R;
                yield return color.G;
                yield return color.B;
                yield return color.A;
            }
        }
    }
}
