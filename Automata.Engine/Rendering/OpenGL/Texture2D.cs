#region

using System;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class Texture2D : Texture
    {
        public Texture2D(string path, WrapMode wrapMode, FilterMode filterMode)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(path);
            image.Mutate(img => img.Flip(FlipMode.Vertical));

            Create<Rgba32>((uint)image.Width, (uint)image.Height, ref image.GetPixelRowSpan(0)[0].R, wrapMode, filterMode, false);

            image.Dispose();
        }

        private void Create<TPixel>(uint width, uint height, ref byte data, WrapMode wrapMode, FilterMode filterMode, bool mipmap)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Bind(TextureUnit.Texture0);

            (InternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType) = GetPixelData<TPixel>();

            GL.TexImage2D(TextureTarget.Texture2D, 0, (int)internalFormat, width, height, 0, pixelFormat, pixelType, ref data);

            AssignTextureParameters(GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));

            if (mipmap) GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        private void AssignTextureParameters(GLEnum wrapMode, GLEnum filterMode)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filterMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filterMode);
        }

        public override void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
