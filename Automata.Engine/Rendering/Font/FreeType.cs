using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Font
{
    public static class FreeType
    {
        private const string _FREETYPE_DLL_IMPORT = "freetype-2.10.4.dll";
        private const CallingConvention _CONVENTION = CallingConvention.Cdecl;

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        internal static extern FreeTypeError FT_Init_FreeType(out IntPtr reference);

        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        internal static extern FreeTypeError FT_Done_Library(IntPtr library);


        [DllImport(_FREETYPE_DLL_IMPORT, CallingConvention = _CONVENTION)]
        internal static extern FreeTypeError FT_New_Face(IntPtr library, string filePath, int faceIndex, out IntPtr reference);

        internal static extern FreeTypeError FT_Load_Char();
    }
}
