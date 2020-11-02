#region

using System;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

#endregion


namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public abstract class Texture : IDisposable
    {
        public enum FilterMode
        {
            Point,
            Bilinear,
            Trilinear
        }

        public enum WrapMode
        {
            Repeat,
            Clamp,
            Mirror
        }

        protected readonly GL GL;
        protected readonly uint Handle;

        protected InternalFormat _InternalFormat;
        protected PixelFormat _PixelFormat;
        protected PixelType _PixelType;

        protected Texture()
        {
            GL = GLAPI.Instance.GL;
            Handle = GL.GenTexture();
        }

        protected void AssignTextureParameters(TextureTarget textureTarget, GLEnum wrapMode, GLEnum filterMode)
        {
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)filterMode);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)filterMode);
        }

        protected static GLEnum GetWrapModeAsGLEnum(WrapMode wrapMode) =>
            wrapMode switch
            {
                WrapMode.Repeat => GLEnum.Repeat,
                WrapMode.Clamp => GLEnum.ClampReadColor,
                WrapMode.Mirror => GLEnum.MirroredRepeat,
                _ => throw new ArgumentOutOfRangeException(nameof(wrapMode))
            };

        protected static GLEnum GetFilterModeAsGLEnum(FilterMode filterMode) =>
            filterMode switch
            {
                FilterMode.Point => GLEnum.Nearest,
                FilterMode.Bilinear => GLEnum.Linear,
                FilterMode.Trilinear => GLEnum.LinearMipmapLinear,
                _ => throw new ArgumentOutOfRangeException(nameof(filterMode))
            };

        protected void AssignPixelFormats<TPixel>() where TPixel : unmanaged, IPixel<TPixel> =>
            (_InternalFormat, _PixelFormat, _PixelType) = GetPixelFormats<TPixel>();

        private static (InternalFormat, PixelFormat, PixelType) GetPixelFormats<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
        {
            if (typeof(TPixel) == typeof(Rgba32)) return (InternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte);
            else if (typeof(TPixel) == typeof(RgbaVector)) return (InternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            else throw new ArgumentOutOfRangeException();
        }

        public abstract void Bind(TextureUnit textureSlot);
        public abstract void Unbind(TextureUnit textureSlot);

        public void Dispose() => GL.DeleteTexture(Handle);
    }
}
