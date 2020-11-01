using System;

namespace Automata.Engine.Rendering.Font
{
    public class FontLibrary : IDisposable
    {
        private readonly IntPtr _Handle;

        private bool _Disposed;

        public IntPtr Handle => _Handle;

        public FontLibrary() => FreeType.ThrowIfNotOk(FreeType.FT_Init_FreeType(out _Handle));

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

            FreeType.FT_Done_Library(Handle);

            _Disposed = true;
        }
    }
}
