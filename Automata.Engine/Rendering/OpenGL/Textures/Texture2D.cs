#region

using System;
using Automata.Engine.Numerics;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

#endregion


namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class Texture2D<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Vector2i Size { get; }

        public Texture2D(Vector2i size, WrapMode wrapMode, FilterMode filterMode, bool mipmap)
        {
            if (Vector2b.Any(size < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");

            Size = size;

            Bind(TextureUnit.Texture0);

            AssignPixelFormats<TPixel>();

            AssignTextureParameters(TextureTarget.Texture2D, GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));
            GL.TexStorage2D(TextureTarget.Texture2D, 1, _InternalFormat, (uint)size.X, (uint)size.Y);

            if (mipmap) GL.GenerateMipmap(TextureTarget.Texture2D);

            GLAPI.CheckForErrorsAndThrow(true);
        }

        public void SetPixels(Vector3i offset, Vector2i size, ref TPixel firstPixel)
        {
            if (Vector3b.Any(offset < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");
            else if (Vector2b.Any(size < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0 and <TexSize");

            Bind(TextureUnit.Texture0);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, offset.X, offset.Y, (uint)size.X, (uint)size.Y, _PixelFormat, _PixelType, ref firstPixel);

            GLAPI.CheckForErrorsAndThrow(true);
        }

        public sealed override void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public sealed override void Unbind(TextureUnit textureSlot) { GL.BindTexture(TextureTarget.Texture2D, 0); }

        public static Texture2D<TPixel> LoadFromFile(string path, WrapMode wrapMode, FilterMode filterMode, bool mipmap)
        {
            using Image<TPixel> image = Image.Load<TPixel>(path);
            image.Mutate(img => img.Flip(FlipMode.Vertical));

            Texture2D<TPixel> texture = new Texture2D<TPixel>(new Vector2i(image.Width, image.Height), wrapMode, filterMode, mipmap);
            texture.SetPixels(Vector3i.Zero, new Vector2i(image.Width, image.Height), ref image.GetPixelRowSpan(0)[0]);

            image.Dispose();

            return texture;
        }
    }
}
