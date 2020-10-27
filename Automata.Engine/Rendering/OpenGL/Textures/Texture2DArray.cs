using System;
using Automata.Engine.Numerics;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class Texture2DArray<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Vector3i Size { get; }

        public Texture2DArray(Vector3i size, WrapMode wrapMode, FilterMode filterMode)
        {
            if (Vector3b.Any(size < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");

            Size = size;
            AssignPixelFormats<TPixel>();

            Bind(TextureUnit.Texture0);

            GL.TexStorage3D(TextureTarget.Texture2DArray, 1, _InternalFormat, (uint)size.X, (uint)size.Y, (uint)size.Z);
            GLAPI.Instance.CheckForErrorsAndThrow(true);
            AssignTextureParameters(TextureTarget.Texture2DArray, GetWrapModeAsGLEnum(wrapMode), GetFilterModeAsGLEnum(filterMode));
        }

        public void SetPixels(Vector3i offset, Vector3i size, Span<TPixel> pixels)
        {
            if (Vector3b.Any(offset < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");
            if (Vector3b.Any(size < 0)) throw new ArgumentOutOfRangeException(nameof(size), "All components must be >=0");

            GL.TextureSubImage3D(Handle, 0, offset.X, offset.Y, offset.Z, (uint)size.X, (uint)size.Y, (uint)size.Z, _PixelFormat,
                _PixelType, pixels);
        }

        public sealed override void Bind(TextureUnit textureSlot)
        {
            GL.BindTexture(TextureTarget.Texture2DArray, Handle);
            GL.ActiveTexture(textureSlot);
        }
    }
}
