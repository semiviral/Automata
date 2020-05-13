#region

using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class VertexBuffer<TDataType> : BufferObject<TDataType> where TDataType : unmanaged
    {
        public VertexBuffer(GL gl) : base(gl, BufferTargetARB.ArrayBuffer)
        {
            if (typeof(TDataType) == typeof(bool))
            {
                throw new ArgumentException("Data type cannot be boolean.", nameof(TDataType));
            }
            else if (typeof(TDataType) == typeof(char))
            {
                throw new ArgumentException("Data type cannot be char.", nameof(TDataType));
            }
        }
    }
}
