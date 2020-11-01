using System;

namespace Automata.Engine.Rendering.Font
{
    public class FontFace : IDisposable
    {
        private readonly IntPtr _Handle;

        private bool _Disposed;

        public IntPtr Handle => _Handle;

        public FontFace(FontLibrary fontLibrary, string path, int faceIndex) =>
            FreeType.ThrowIfNotOk(FreeType.FT_New_Face(fontLibrary.Handle, path, faceIndex, out _Handle));

        public void SetPixelSize(uint width, uint height) => FreeType.ThrowIfNotOk(FreeType.FT_Set_Pixel_Sizes(Handle, width, height));

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool dispose)
        {
            if (_Disposed || !dispose)
            {
                return;
            }

            FreeType.FT_Done_Face(Handle);

            _Disposed = true;
        }
    }
}
