using System;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class Texture2D<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Vector2<int> Size { get; }

        public Texture2D(Vector2<int> size, WrapMode wrapMode, FilterMode filterMode, bool mipmap) :
            this(GLAPI.Instance.GL, size, wrapMode, filterMode, mipmap) { }

        public Texture2D(GL gl, Vector2<int> size, WrapMode wrapMode, FilterMode filterMode, bool mipmap) : base(gl, TextureTarget.Texture2D)
        {
            if (Vector.Any(size < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");
            }

            Size = size;

            AssignPixelFormats<TPixel>();
            AssignTextureParameters(GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));
            GL.TextureStorage2D(Handle, 1, _InternalFormat, (uint)size.X, (uint)size.Y);

            if (mipmap)
            {
                GL.GenerateTextureMipmap(Handle);
            }
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

            GL.TextureSubImage2D(Handle, 0, offset.X, offset.Y, (uint)size.X, (uint)size.Y, _PixelFormat, _PixelType, pixels);
        }

        public sealed override void Bind(uint unit) => GL.BindTextureUnit(unit, Handle);

        public static Texture2D<TPixel> Load(string path, WrapMode wrapMode, FilterMode filterMode, bool mipmap)
        {
            using Image<TPixel> image = Image.Load<TPixel>(path);
            image.Mutate(img => img.Flip(FlipMode.Vertical));
            Texture2D<TPixel> texture = new Texture2D<TPixel>(new Vector2<int>(image.Width, image.Height), wrapMode, filterMode, mipmap);
            texture.SetPixels(Vector3<int>.Zero, new Vector2<int>(image.Width, image.Height), image.GetPixelSpan());

            return texture;
        }
    }
}
