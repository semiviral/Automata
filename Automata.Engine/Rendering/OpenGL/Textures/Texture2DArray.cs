using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class Texture2DArray<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Texture2DArray(uint width, uint height, uint depth, WrapMode wrapMode, FilterMode filterMode)
        {
            Bind(TextureUnit.Texture0);

            (InternalFormat internalFormat, PixelFormat _, PixelType _) = GetPixelData<TPixel>();

            GL.TextureStorage3D((uint)TextureTarget.Texture2DArray, 0, internalFormat, width, height, depth);
        }


        public void SetPixel()

        public sealed override void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2DArray, Handle);
        }
    }
}
