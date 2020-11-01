using System;

namespace Automata.Engine.Rendering.Font
{
    public class FontLibrary : IDisposable
    {
        private readonly IntPtr _Handle;

        private bool _Disposed;

        public FontLibrary()
        {
            FreeTypeError error = FreeType.FT_Init_FreeType(out _Handle);

            if (error is not FreeTypeError.Ok)
            {
                throw new FreeTypeException(error);
            }
        }

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

            FreeType.FT_Done_Library(_Handle);

            _Disposed = true;
        }
    }
}
