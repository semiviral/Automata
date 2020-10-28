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
    public class Texture3D<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Vector3i Size { get; }

        public Texture3D(Vector3i size, WrapMode wrapMode, FilterMode filterMode, bool mipmap)
        {
            if (Vector3b.Any(size < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");

            Size = size;

            Bind(TextureUnit.Texture0);

            AssignPixelFormats<TPixel>();

            AssignTextureParameters(TextureTarget.Texture3D, GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));
            GL.TexStorage3D(TextureTarget.Texture3D, 1, _InternalFormat, (uint)size.X, (uint)size.Y, (uint)size.Z);

            if (mipmap) GL.GenerateMipmap(TextureTarget.Texture3D);

            GLAPI.CheckForErrorsAndThrow(true);
        }

        public void SetPixels(Vector3i offset, Vector3i size, ref TPixel firstPixel)
        {
            if (Vector3b.Any(offset < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");
            else if (Vector3b.Any(size < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0 and <TexSize");

            Bind(TextureUnit.Texture0);

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, offset.X, offset.Y, offset.Z, (uint)size.X, (uint)size.Y, (uint)size.Z, _PixelFormat, _PixelType,
                ref firstPixel);

            GLAPI.CheckForErrorsAndThrow(true);
        }

        public sealed override void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture3D, Handle);
        }

        public sealed override void Unbind(TextureUnit textureSlot)
        {
            GL.BindTexture(TextureTarget.Texture3D, 0);
        }
    }
}
