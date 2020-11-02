using System;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.Fonts.FreeType;

namespace Automata.Engine.Rendering.Fonts
{
    public class GlyphBitmap : IDisposable
    {
        private IntPtr _Handle;
        private FreeTypeBitmap _Bitmap;
        private FontLibrary _Library;
        private bool _Disposed;
        private readonly bool _User;

        private FreeTypeBitmap Bitmap
        {
            get
            {
                if (_Disposed) throw new ObjectDisposedException(nameof(GlyphBitmap));

                return _Bitmap;
            }
        }

        public IntPtr Handle
        {
            get => _Handle;
            set
            {
                if (_Disposed) throw new ObjectDisposedException(nameof(GlyphBitmap));

                _Handle = value;
                _Bitmap = Marshal.PtrToStructure<FreeTypeBitmap>(value);
            }
        }

        public int Rows => Bitmap.Rows;
        public int Width => Bitmap.Width;
        public int Pitch => Bitmap.Pitch;

        internal GlyphBitmap(FontLibrary fontLibrary)
        {
            IntPtr handle = Marshal.AllocHGlobal(Marshal.SizeOf<FreeTypeBitmap>());
            FreeType.FreeType.FT_Bitmap_New(handle);
            Handle = handle;

            _Library = fontLibrary;
            _User = true;
        }

        internal GlyphBitmap(IntPtr handle, FreeTypeBitmap bitmap, FontLibrary fontLibrary)
        {
            Handle = handle;
            _Bitmap = bitmap;
            _Library = fontLibrary;
        }

        public unsafe Span<byte> Buffer()
        {
            if (Pitch < 0) throw new ArgumentOutOfRangeException(nameof(Pitch), "Pitch is negative.");

            return new Span<byte>(Bitmap.Buffer.ToPointer(), Rows * Pitch);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose)
        {
            if (_Disposed || !dispose) return;

            _Disposed = true;

            if (_User)
            {
                FreeType.FreeType.FT_Bitmap_Done(_Library.Handle, Handle);
                Marshal.FreeHGlobal(Handle);
            }

            Handle = IntPtr.Zero;
            _Library = null!;
        }
    }
}