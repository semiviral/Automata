#region

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

            AssignPixelFormats<TPixel>();

            Bind(TextureUnit.Texture0);

            GL.TexImage2D(TextureTarget.Texture2D, 0, (int)_InternalFormat, (uint)image.Width, (uint)image.Height, 0, _PixelFormat, _PixelType,
                ref image.GetPixelRowSpan(0)[0]);

            ConfigureTexture(wrapMode, filterMode, mipmap);

            image.Dispose();
        }

        private void ConfigureTexture(WrapMode wrapMode, FilterMode filterMode, bool mipmap)
        {
            AssignTextureParameters(TextureTarget.Texture2D, GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));

            if (mipmap) GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        public sealed override void Bind(TextureUnit textureSlot)
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GL.ActiveTexture(textureSlot);
        }
    }
}
