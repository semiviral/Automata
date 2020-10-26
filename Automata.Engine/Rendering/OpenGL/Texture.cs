#region

using System;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

#endregion


namespace Automata.Engine.Rendering.OpenGL
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

        protected Texture()
        {
            GL = GLAPI.Instance.GL;
            Handle = GL.GenTexture();
        }

        public abstract void Bind(TextureUnit textureSlot);

        public void Dispose() => GL.DeleteTexture(Handle);


        #region Static Methods

        protected static (InternalFormat, PixelFormat, PixelType) GetPixelData<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
        {
            if (typeof(TPixel) == typeof(Rgba32)) return (InternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte);
            else if (typeof(TPixel) == typeof(RgbaVector)) return (InternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            else throw new ArgumentOutOfRangeException();
        }

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
    }
}
