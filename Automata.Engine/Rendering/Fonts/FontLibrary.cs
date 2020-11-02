using System;
using Automata.Engine.Rendering.Fonts.FreeTypePrimitives;

namespace Automata.Engine.Rendering.Fonts
{
    public class FontLibrary : IDisposable
    {
        private readonly IntPtr _Handle;

        private bool _Disposed;

        public IntPtr Handle
        {
            get
            {
                if (_Disposed) throw new ObjectDisposedException(nameof(FontFace));
                return _Handle;
            }
        }

        public FontLibrary() => FreeType.ThrowIfNotOk(FreeType.FT_Init_FreeType(out _Handle));

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool dispose)
        {
            if (_Disposed || !dispose) return;

            FreeType.FT_Done_Library(Handle);

            _Disposed = true;
        }
    }
}
