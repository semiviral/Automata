using System;
using Automata.Engine.Numerics;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class Texture2DArray<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Vector3<int> Size { get; }

        public Texture2DArray(Vector3<int> size, WrapMode wrapMode, FilterMode filterMode) : this(GLAPI.Instance.GL, size, wrapMode, filterMode) { }

        public Texture2DArray(GL gl, Vector3<int> size, WrapMode wrapMode, FilterMode filterMode) : base(gl, TextureTarget.Texture2DArray)
        {
            if (Vector.Any(size < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");
            }

            Size = size;

            AssignPixelFormats<TPixel>();
            AssignTextureParameters(GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));
            GL.TextureStorage3D(Handle, 1, _InternalFormat, (uint)size.X, (uint)size.Y, (uint)size.Z);
        }

        public void SetPixels(Vector3<int> offset, Vector2<int> size, ReadOnlySpan<TPixel> pixels)
        {
            if (Vector.Any(offset < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");
            }
            else if (Vector.Any(size < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0 and <TexSize");
            }

            GL.TextureSubImage3D(Handle, 0, offset.X, offset.Y, offset.Z, (uint)size.X, (uint)size.Y, 1u, _PixelFormat, _PixelType, pixels);
        }

        public sealed override void Bind(uint unit) => GL.BindTextureUnit(unit, Handle);
    }
}
