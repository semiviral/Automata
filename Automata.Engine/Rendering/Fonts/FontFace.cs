using System;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.Fonts.FreeType;

namespace Automata.Engine.Rendering.Fonts
{
    public class FontFace : IDisposable
    {
        private IntPtr _Handle;
        private FreeTypeFace _Face;

        private bool _Disposed;

        private FreeTypeFace Face
        {
            get
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(FontFace));
                }

                return _Face;
            }
        }

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
            private set
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(FontFace));
                }

                _Handle = value;
                _Face = Marshal.PtrToStructure<FreeTypeFace>(value);
            }
        }

        public string? FontFamily => Marshal.PtrToStringAnsi(Face.FamilyName);
        public string? FontStyle => Marshal.PtrToStringAnsi(Face.StyleName);
        public long FaceCount => Face.FacesCount;
        public long FaceIndex => Face.FaceIndex;
        public long GlyphCount => Face.GlyphCount;


        public FontFace(FontLibrary fontLibrary, string path, int faceIndex)
        {
            FreeType.FreeType.ThrowIfNotOk(FreeType.FreeType.FT_New_Face(fontLibrary.Handle, path, faceIndex, out IntPtr handle));
            Handle = handle;
        }

        public void SetPixelSize(uint width, uint height) => FreeType.FreeType.ThrowIfNotOk(FreeType.FreeType.FT_Set_Pixel_Sizes(Handle, width, height));

        public void SelectCharmap(FontEncoding encoding) => FreeType.FreeType.ThrowIfNotOk(FreeType.FreeType.FT_Select_Charmap(Handle, encoding));

        public uint FirstCharacterCode(out uint glyphIndex) => FreeType.FreeType.FT_Get_First_Char(Handle, out glyphIndex);
        public uint NextCharacterCode(out uint glyphIndex) => FreeType.FreeType.FT_Get_Next_Char(Handle, out glyphIndex);

        public void LoadCharacter(uint characterCode, LoadFlags loadFlags) =>
            FreeType.FreeType.ThrowIfNotOk(FreeType.FreeType.FT_Load_Char(Handle, characterCode, loadFlags));

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

            FreeType.FreeType.FT_Done_Face(Handle);

            _Disposed = true;
        }
    }
}
