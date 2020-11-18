using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    public static class FreeType
    {
        private const string _FREETYPE_DLL_IMPORT = @".\References\freetype-2.10.4.dll";
        private const CallingConvention _CONVENTION = CallingConvention.Cdecl;

        public static void ThrowIfNotOk(FreeTypeError error)
        {
            if (error is not FreeTypeError.Ok)
            {
                throw new FreeTypeException(error);
            }
        }

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern void FT_Library_Version(IntPtr library, out int major, out int minor, out int patch);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Init_FreeType(out IntPtr handle);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Done_FreeType(IntPtr handle);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Done_Library(IntPtr library);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern FreeTypeError FT_New_Face(IntPtr library, string filePath, int faceIndex, out IntPtr handle);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Done_Face(IntPtr face);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Set_Pixel_Sizes(IntPtr face, uint width, uint height);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Select_Charmap(IntPtr face, FontEncoding fontEncoding);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern uint FT_Get_First_Char(IntPtr face, out uint glyphIndex);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern uint FT_Get_Next_Char(IntPtr face, uint currentCharacterCode, out uint glyphIndex);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern uint FT_Get_Char_Index(IntPtr face, uint charCode);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Load_Char(IntPtr face, uint charCode, LoadFlags loadFlags);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Load_Glyph(IntPtr face, uint glyphIndex, LoadFlags loadFlags);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Load_Sfnt_Table(IntPtr face, uint tag, int offset, IntPtr buffer, ref uint length);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern void FT_Bitmap_New(IntPtr preAllocated);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Bitmap_Done(IntPtr library, IntPtr bitmap);
    }
}
