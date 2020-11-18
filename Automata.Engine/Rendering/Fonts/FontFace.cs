using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Automata.Engine.Rendering.Fonts.FreeTypePrimitives;

namespace Automata.Engine.Rendering.Fonts
{
    public class FontFace : IDisposable
    {
        private readonly FontLibrary _Library;
        private readonly HashSet<Rune> _AvailableCharacters;

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

        public FaceFlags FaceFlags => Face.FaceFlags;
        public StyleFlags StyleFlags => Face.StyleFlags;

        public int FaceCount => Face.FacesCount;
        public int FaceIndex => Face.FaceIndex;
        public int GlyphCount => Face.GlyphCount;

        public ushort UnitsPerEM => Face.UnitsPerEM;
        public short Ascender => Face.Ascender;
        public short Descender => Face.Descender;
        public short Height => Face.Height;
        public short MaxAdvanceWidth => Face.MaxAdvanceWidth;
        public short MaxAdvanceHeight => Face.MaxAdvanceHeight;
        public short UnderlinePosition => Face.UnderlinePosition;
        public short UnderlineThickness => Face.UnderlineThickness;

        public IReadOnlyCollection<Rune> AvailableCharacters => _AvailableCharacters;

        public FontFace(FontLibrary fontLibrary, string path, uint faceIndex)
        {
            FreeType.ThrowIfNotOk(FreeType.FT_New_Face(fontLibrary.Handle, path, (int)faceIndex, out IntPtr handle));
            _Library = fontLibrary;
            Handle = handle;

            _AvailableCharacters = new HashSet<Rune>();
        }

        public Glyph Glyph() => new Glyph(_Face.Glyph, _Library, this);

        public void SetPixelSize(uint width, uint height) => FreeType.ThrowIfNotOk(FreeType.FT_Set_Pixel_Sizes(Handle, width, height));
        public void SelectCharmap(FontEncoding encoding) => FreeType.ThrowIfNotOk(FreeType.FT_Select_Charmap(Handle, encoding));
        public uint FirstCharacterCode(out uint glyphIndex) => FreeType.FT_Get_First_Char(Handle, out glyphIndex);
        public uint NextCharacterCode(uint charCode, out uint glyphIndex) => FreeType.FT_Get_Next_Char(Handle, charCode, out glyphIndex);

        public void LoadCharacter(uint charCode, LoadFlags loadFlags, LoadTarget loadTarget) =>
            FreeType.ThrowIfNotOk(FreeType.FT_Load_Char(Handle, charCode, (LoadFlags)((int)loadFlags | (int)loadTarget)));

        public void LoadGlyph(uint glyphIndex, LoadFlags loadFlags, LoadTarget loadTarget) =>
            FreeType.ThrowIfNotOk(FreeType.FT_Load_Glyph(Handle, glyphIndex, (LoadFlags)((int)loadFlags | (int)loadTarget)));

        public uint GetGlyphIndex(uint charCode) => FreeType.FT_Get_Char_Index(Handle, charCode);

        /// <summary>
        ///     Iterates the font and adds all of the valid character codes to an internal collection.
        /// </summary>
        /// <remarks>
        ///     For more verbose fonts (i.e. fonts that implement many characters) this will result in one iteration per character.
        /// </remarks>
        public void ParseAvailableCharacters()
        {
            uint charCode = FirstCharacterCode(out uint glyphIndex);

            while (glyphIndex > 0u)
            {
                _AvailableCharacters.Add(new Rune(charCode));
                charCode = NextCharacterCode(charCode, out glyphIndex);
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

            FreeType.FT_Done_Face(Handle);

            _Disposed = true;
        }
    }
}
