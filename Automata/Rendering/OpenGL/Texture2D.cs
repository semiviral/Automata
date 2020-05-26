#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class Texture2D : Texture, IDisposable
    {
        public Texture2D(uint width, uint height, TextureFormat textureFormat)
            : this(width, height, textureFormat, WrapMode.Repeat) { }

        public Texture2D(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode)
            : this(width, height, textureFormat, wrapMode, FilterMode.Point) { }

        public Texture2D(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode)
            : this(width, height, textureFormat, wrapMode, filterMode, true) { }

        public Texture2D(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode, bool mipmapping)
            : base()
        {
            Create(width, height, textureFormat, wrapMode, filterMode, mipmapping);
        }

        private void Create(uint width, uint height, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode, bool mipmapping)
        {
            Bind(TextureUnit.Texture0);

            (InternalFormat internalFormat, PixelFormat _) = GetInternalTextureFormatRepresentation(textureFormat);

            GL.TexStorage2D(TextureTarget.Texture2D, 0, internalFormat, width, height);

            GLEnum wrapModeGl = GetWrapModeAsGLEnum(wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapModeGl);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapModeGl);

            GLEnum filterModeGl = GetFilterModeAsGLEnum(filterMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filterModeGl);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filterModeGl);

            if (mipmapping)
            {
                GL.GenerateMipmap(TextureTarget.Texture2D);
            }
        }

        public override void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
