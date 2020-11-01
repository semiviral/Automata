using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Font
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
        public static extern FreeTypeError FT_Init_FreeType(out IntPtr reference);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Done_Library(IntPtr library);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_New_Face(IntPtr library, string filePath, int faceIndex, out IntPtr reference);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Done_Face(IntPtr face);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Set_Pixel_Sizes(IntPtr face, uint width, uint height);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        public static extern FreeTypeError FT_Load_Sfnt_Table(IntPtr face, uint tag, int offset, IntPtr buffer, ref uint length);
    }
}
