using System;
using Automata.Engine.Numerics;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class Texture2DArray<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Texture2DArray(Vector3i size, WrapMode wrapMode, FilterMode filterMode)
        {
            if (Vector3b.Any(size < Vector3i.Zero)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");

            Bind(TextureUnit.Texture0);

            (InternalFormat internalFormat, PixelFormat _, PixelType _) = GetPixelData<TPixel>();

            //GL.TextureStorage3D((uint)TextureTarget.Texture2DArray, 0, internalFormat, width, height, depth);
        }


        public void SetPixel(Vector3i offset, Vector2i size)
        {
            //GL.TextureSubImage3D();
        }

        public sealed override void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2DArray, Handle);
        }
    }
}
