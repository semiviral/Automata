using System;
using Automata.Engine.Rendering.Fonts.FreeTypePrimitives;

namespace Automata.Engine.Rendering.Fonts
{
    public class FontLibrary : IDisposable
    {
        private readonly IntPtr _Handle;

        private bool _Disposed;
        private bool _CustomMemory;

        public IntPtr Handle
        {
            get
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(FontFace));
                }

                return _Handle;
            }
        }

        public FontLibrary() => FreeType.ThrowIfNotOk(FreeType.FT_Init_FreeType(out _Handle));

        public Version Version()
        {
            FreeType.FT_Library_Version(Handle, out int major, out int minor, out int patch);
            return new Version(major, minor, patch);
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

            FreeType.ThrowIfNotOk(_CustomMemory ? FreeType.FT_Done_Library(Handle) : FreeType.FT_Done_FreeType(Handle));

            _Disposed = true;
        }
    }
}
