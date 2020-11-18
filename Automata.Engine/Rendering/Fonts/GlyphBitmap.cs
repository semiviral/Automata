using System;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.Fonts.FreeTypePrimitives;

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
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(GlyphBitmap));
                }

                return _Bitmap;
            }
        }

        public IntPtr Handle
        {
            get => _Handle;
            set
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(GlyphBitmap));
                }

                _Handle = value;
                _Bitmap = Marshal.PtrToStructure<FreeTypeBitmap>(value);
            }
        }

        public uint Rows => Bitmap.Rows;
        public uint Width => Bitmap.Width;
        public int Pitch => Bitmap.Pitch;

        internal GlyphBitmap(FontLibrary fontLibrary)
        {
            IntPtr handle = Marshal.AllocHGlobal(Marshal.SizeOf<FreeTypeBitmap>());
            FreeType.FT_Bitmap_New(handle);
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
            if (Pitch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Pitch), "Pitch is negative.");
            }

            return new Span<byte>(Bitmap.Buffer.ToPointer(), (int)(Rows * Pitch));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose)
        {
            if (_Disposed || !dispose)
            {
                return;
            }

            _Disposed = true;

            if (_User)
            {
                FreeType.FT_Bitmap_Done(_Library.Handle, Handle);
                Marshal.FreeHGlobal(Handle);
            }

            Handle = IntPtr.Zero;
            _Library = null!;
        }
    }
}
