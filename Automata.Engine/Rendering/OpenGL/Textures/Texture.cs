using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public abstract class Texture : OpenGLObject, IEquatable<Texture>
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

        protected InternalFormat _InternalFormat;
        protected PixelFormat _PixelFormat;
        protected PixelType _PixelType;

        protected unsafe Texture(GL gl, TextureTarget textureTarget) : base(gl)
        {
            uint handle = 0;
            GL.CreateTextures(textureTarget, 1, &handle);
            Handle = handle;
        }

        protected void AssignTextureParameters(GLEnum wrapMode, GLEnum filterMode)
        {
            GL.TextureParameter(Handle, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TextureParameter(Handle, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)filterMode);
            GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int)filterMode);
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
            if (typeof(TPixel) == typeof(Rgba32))
            {
                return (InternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte);
            }
            else if (typeof(TPixel) == typeof(RgbaVector))
            {
                return (InternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }


        #region IDisposable

        protected override void SafeDispose() => GL.DeleteTexture(Handle);

        #endregion


        #region Binding

        /// <summary>
        ///     Binds the texture to the given texture unit.
        /// </summary>
        /// <param name="unit">Texture unit for binding the texture to.</param>
        public abstract void Bind(uint unit);

        /// <summary>
        /// </summary>
        /// <param name="gl">The <see cref="GL" /> to queue the command from.</param>
        /// <param name="first">First texture unit to start binding from.</param>
        /// <param name="textures">Textures to bind.</param>
        [SkipLocalsInit]
        public static unsafe void BindMany(GL gl, uint first, ICollection<Texture> textures)
        {
            Span<uint> handles = stackalloc uint[textures.Count];

            int index = 0;

            foreach (Texture texture in textures)
            {
                handles[index] = texture.Handle;
                index += 1;
            }

            BindManyInternal(gl, first, handles);
        }

        /// <summary>
        ///     Binds many textures in one GL call.
        /// </summary>
        /// <param name="gl">The <see cref="GL" /> to queue the command from.</param>
        /// <param name="first">The first texture unit to start binding from.</param>
        /// <param name="textures">The textures to bind.</param>
        [SkipLocalsInit]
        public static unsafe void BindMany(GL gl, uint first, Span<Texture> textures)
        {
            Span<uint> handles = stackalloc uint[textures.Length];

            for (int index = 0; index < textures.Length; index++)
            {
                handles[index] = textures[index].Handle;
            }

            BindManyInternal(gl, first, handles);
        }

        private static void BindManyInternal(GL gl, uint first, Span<uint> handles) => gl.BindTextures(first, (uint)handles.Length, handles);

        #endregion


        #region IEquatable

        public bool Equals(Texture? other) => other is not null && (other.Handle == Handle);
        public override bool Equals(object? obj) => obj is Texture texture && Equals(texture);

        public override int GetHashCode() => (int)Handle;

        public static bool operator ==(Texture? left, Texture? right) => Equals(left, right);
        public static bool operator !=(Texture? left, Texture? right) => !Equals(left, right);

        #endregion
    }
}
