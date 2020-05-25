#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public enum TextureFormat
    {
        R32,
        RGB32,
        RGBA32,
        Depth32
    }

    public enum WrapMode
    {
        Repeat,
        Clamp,
        Mirror
    }

    public enum FilterMode
    {
        Point,
        Bilinear,
        Trilinear
    }

    public class Texture2D : IDisposable
    {
        private static (InternalFormat internalFormat, PixelFormat pixelFormat) GetInternalTextureFormatRepresentation(TextureFormat textureFormat) =>
            textureFormat switch
            {
                TextureFormat.R32 => (InternalFormat.R32f, PixelFormat.Red),
                TextureFormat.RGB32 => (InternalFormat.Rgb32f, PixelFormat.Rgb),
                TextureFormat.RGBA32 => (InternalFormat.Rgba32f, PixelFormat.Rgba),
                TextureFormat.Depth32 => (InternalFormat.DepthComponent32f, PixelFormat.DepthComponent),
                _ => throw new ArgumentOutOfRangeException(nameof(textureFormat))
            };

        private static GLEnum GetWrapModeAsGLEnum(WrapMode wrapMode) =>
            wrapMode switch
            {
                WrapMode.Repeat => GLEnum.Repeat,
                WrapMode.Clamp => GLEnum.ClampReadColor,
                WrapMode.Mirror => GLEnum.MirroredRepeat,
                _ => throw new ArgumentOutOfRangeException(nameof(wrapMode))
            };

        private static GLEnum GetFilerModeAsGLEnum(FilterMode filterMode) =>
            filterMode switch
            {
                FilterMode.Point => GLEnum.Nearest,
                FilterMode.Bilinear => GLEnum.Linear,
                FilterMode.Trilinear => GLEnum.LinearMipmapLinear,
                _ => throw new ArgumentOutOfRangeException(nameof(filterMode))
            };

        private readonly uint _Handle;
        private readonly GL _GL;

        public Texture2D(uint width, uint height, TextureFormat textureFormat)
            : this(width, height, textureFormat, WrapMode.Repeat) { }

        public Texture2D(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode)
            : this(width, height, textureFormat, wrapMode, FilterMode.Point) { }

        public Texture2D(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode)
            : this(width, height, textureFormat, wrapMode, filterMode, true) { }

        public Texture2D(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode, bool mipMapping)
        {
            _GL = GLAPI.Instance.GL;
            _Handle = _GL.GenTexture();

            Create(width, height, textureFormat, wrapMode, filterMode, mipMapping);
        }

        private void Create(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode, bool mipMapping)
        {
            Bind(TextureUnit.Texture0);

            (InternalFormat internalFormat, PixelFormat pixelFormat) = GetInternalTextureFormatRepresentation(textureFormat);

            _GL.TexStorage2D(TextureTarget.Texture2D, 0, internalFormat, width, height);

            GLEnum wrapModeGl = GetWrapModeAsGLEnum(wrapMode);
            _GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapModeGl);
            _GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapModeGl);

            GLEnum pointFilerGl = GetFilerModeAsGLEnum(filterMode);
            _GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)pointFilerGl);
            _GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)pointFilerGl);

            if (mipMapping)
            {
                _GL.GenerateMipmap(TextureTarget.Texture2D);
            }
        }

        public void Bind(TextureUnit textureSlot)
        {
            _GL.ActiveTexture(textureSlot);
            _GL.BindTexture(TextureTarget.Texture2D, _Handle);
        }

        public void Dispose()
        {
            _GL.DeleteTexture(_Handle);
        }
    }
}
