#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class Texture3D : Texture, IDisposable
    {
        public Texture3D(uint width, uint height, uint depth, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode, bool mipmapping)
        {
            Create(width, height, depth, textureFormat, wrapMode, filterMode, mipmapping);
        }

        private void Create(uint width, uint height, uint depth, TextureFormat textureFormat, WrapMode wrapMode, FilterMode filterMode,
            bool mipmapping)
        {
            Bind(TextureUnit.Texture0);

            (InternalFormat internalFormat, PixelFormat _) = GetInternalTextureFormatRepresentation(textureFormat);

            GL.TexStorage3D(TextureTarget.Texture3D, 0, internalFormat, width, height, depth);

            GLEnum wrapModeGl = GetWrapModeAsGLEnum(wrapMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)wrapModeGl);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)wrapModeGl);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)wrapModeGl);

            GLEnum filterModeGl = GetFilterModeAsGLEnum(filterMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)filterModeGl);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)filterModeGl);

            if (mipmapping)
            {
                GL.GenerateMipmap(TextureTarget.Texture3D);
            }
        }

        public void Bind(TextureUnit textureSlot)
        {
            GL.ActiveTexture(textureSlot);
            GL.BindTexture(TextureTarget.Texture3D, Handle);
        }

        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }
    }
}
