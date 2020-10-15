#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Engine.Rendering.OpenGL
{
    public abstract class Texture
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

        #region Static Methods

        public static (InternalFormat internalFormat, PixelFormat pixelFormat) GetInternalTextureFormatRepresentation(TextureFormat textureFormat) =>
            textureFormat switch
            {
                TextureFormat.R32 => (InternalFormat.R32f, PixelFormat.Red),
                TextureFormat.RGB32 => (InternalFormat.Rgb32f, PixelFormat.Rgb),
                TextureFormat.RGBA32 => (InternalFormat.Rgba32f, PixelFormat.Rgba),
                TextureFormat.Depth32 => (InternalFormat.DepthComponent32f, PixelFormat.DepthComponent),
                _ => throw new ArgumentOutOfRangeException(nameof(textureFormat))
            };

        public static GLEnum GetWrapModeAsGLEnum(WrapMode wrapMode) =>
            wrapMode switch
            {
                WrapMode.Repeat => GLEnum.Repeat,
                WrapMode.Clamp => GLEnum.ClampReadColor,
                WrapMode.Mirror => GLEnum.MirroredRepeat,
                _ => throw new ArgumentOutOfRangeException(nameof(wrapMode))
            };

        public static GLEnum GetFilterModeAsGLEnum(FilterMode filterMode) =>
            filterMode switch
            {
                FilterMode.Point => GLEnum.Nearest,
                FilterMode.Bilinear => GLEnum.Linear,
                FilterMode.Trilinear => GLEnum.LinearMipmapLinear,
                _ => throw new ArgumentOutOfRangeException(nameof(filterMode))
            };

        #endregion

        protected readonly GL GL;
        protected readonly uint Handle;

        protected Texture()
        {
            GL = GLAPI.Instance.GL;
            Handle = GL.GenTexture();
        }

        public abstract void Bind(TextureUnit textureSlot);

        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }
    }
}
