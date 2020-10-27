#region

using System;
using System.Runtime.InteropServices;
using Automata.Engine.Extensions;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

#endregion


namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class Texture2D<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Texture2D(string path, WrapMode wrapMode, FilterMode filterMode, bool mipmap)
        {
            Image<TPixel> image = Image.Load<TPixel>(path);
            image.Mutate(img => img.Flip(FlipMode.Vertical));

            UploadImage((uint)image.Width, (uint)image.Height, ref image.GetPixelRowSpan(0)[0]);

            ConfigureTexture(wrapMode, filterMode, mipmap);

            image.Dispose();
        }

        private void UploadImage(uint width, uint height, ref TPixel data)
        {
            Bind(TextureUnit.Texture0);

            (InternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType) = GetPixelData<TPixel>();

            byte firstPixelByte = data.GetValue<TPixel, byte>(0);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (int)internalFormat, width, height, 0, pixelFormat, pixelType, ref firstPixelByte);
        }

        private void ConfigureTexture(WrapMode wrapMode, FilterMode filterMode, bool mipmap)
        {
            AssignTextureParameters(TextureTarget.Texture2D, GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));

            if (mipmap) GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        public override void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
