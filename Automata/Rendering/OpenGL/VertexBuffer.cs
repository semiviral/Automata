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
            SetBufferData(TranslateVector3Data(vertices.ToArray()));
        }

        public void SetBufferData(IEnumerable<Vector3> vertices, IEnumerable<Color64> colors)
        {
            SetBufferData(TranslateVector3AndColorData(vertices.ToArray(), colors.ToArray()));
        }

        private static float[] TranslateVector3Data(Vector3[] vertices)
        {
            if (vertices == null)
            {
                throw new NullReferenceException($"Argument '{nameof(vertices)}' cannot be null.");
            }

            float[] finalArray = new float[vertices.Length * 3];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].CopyTo(finalArray, i * 3);
            }

            return finalArray;
        }

        private static float[] TranslateVector3AndColorData(Vector3[] vertices, Color64[] colors)
        {
            if (vertices == null)
            {
                throw new NullReferenceException($"Argument '{nameof(vertices)}' cannot be null.");
            }
            else if (colors == null)
            {
                throw new NullReferenceException($"Argument '{nameof(colors)}' cannot be null.");
            }
            else if (vertices.Length != colors.Length)
            {
                throw new ArgumentException("Given arrays must match in size.");
            }

            int finalArrayLength = vertices.Length * 7;
            float[] finalArray = new float[finalArrayLength];

            for (int i = 0; i < vertices.Length; i++)
            {
                int adjustedIndex = i * 7;
                vertices[i].CopyTo(finalArray, adjustedIndex);
                colors[i].CopyTo(finalArray, adjustedIndex + 3);
            }

            return finalArray;
        }
    }
}
