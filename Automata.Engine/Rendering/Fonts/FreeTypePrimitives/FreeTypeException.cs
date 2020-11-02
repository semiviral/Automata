using System;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    /// <summary>
    ///     An exception that gets thrown when FreeType returns an error code.
    /// </summary>
    public class FreeTypeException : Exception
    {
        public FreeTypeError Error { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FreeTypeException" /> class.
        /// </summary>
        /// <param name="error">The error returned by FreeType.</param>
        public FreeTypeException(FreeTypeError error) : base("FreeType error: " + GetErrorMessage(error)) => Error = error;

        private static string GetErrorMessage(FreeTypeError error)
        {
            return error switch
            {
                FreeTypeError.Ok => "No error.",
                FreeTypeError.CannotOpenResource => "Cannot open resource.",
                FreeTypeError.UnknownFileFormat => "Unknown file format.",
                FreeTypeError.InvalidFileFormat => "Broken file.",
                FreeTypeError.InvalidVersion => "Invalid FreeType version.",
                FreeTypeError.LowerModuleVersion => "Module version is too low.",
                FreeTypeError.InvalidArgument => "Invalid argument.",
                FreeTypeError.UnimplementedFeature => "Unimplemented feature.",
                FreeTypeError.InvalidTable => "Broken table.",
                FreeTypeError.InvalidOffset => "Broken offset within table.",
                FreeTypeError.ArrayTooLarge => "Array allocation size too large.",
                FreeTypeError.InvalidGlyphIndex => "Invalid glyph index.",
                FreeTypeError.InvalidCharacterCode => "Invalid character code.",
                FreeTypeError.InvalidGlyphFormat => "Unsupported glyph image format.",
                FreeTypeError.CannotRenderGlyph => "Cannot render this glyph format.",
                FreeTypeError.InvalidOutline => "Invalid outline.",
                FreeTypeError.InvalidComposite => "Invalid composite glyph.",
                FreeTypeError.TooManyHints => "Too many hints.",
                FreeTypeError.InvalidPixelSize => "Invalid pixel size.",
                FreeTypeError.InvalidHandle => "Invalid object handle.",
                FreeTypeError.InvalidLibraryHandle => "Invalid library handle.",
                FreeTypeError.InvalidDriverHandle => "Invalid module handle.",
                FreeTypeError.InvalidFaceHandle => "Invalid face handle.",
                FreeTypeError.InvalidSizeHandle => "Invalid size handle.",
                FreeTypeError.InvalidSlotHandle => "Invalid glyph slot handle.",
                FreeTypeError.InvalidCharMapHandle => "Invalid charmap handle.",
                FreeTypeError.InvalidCacheHandle => "Invalid cache manager handle.",
                FreeTypeError.InvalidStreamHandle => "Invalid stream handle.",
                FreeTypeError.TooManyDrivers => "Too many modules.",
                FreeTypeError.TooManyExtensions => "Too many extensions.",
                FreeTypeError.OutOfMemory => "Out of memory.",
                FreeTypeError.UnlistedObject => "Unlisted object.",
                FreeTypeError.CannotOpenStream => "Cannot open stream.",
                FreeTypeError.InvalidStreamSeek => "Invalid stream seek.",
                FreeTypeError.InvalidStreamSkip => "Invalid stream skip.",
                FreeTypeError.InvalidStreamRead => "Invalid stream read.",
                FreeTypeError.InvalidStreamOperation => "Invalid stream operation.",
                FreeTypeError.InvalidFrameOperation => "Invalid frame operation.",
                FreeTypeError.NestedFrameAccess => "Nested frame access.",
                FreeTypeError.InvalidFrameRead => "Invalid frame read.",
                FreeTypeError.RasterUninitialized => "Raster uninitialized.",
                FreeTypeError.RasterCorrupted => "Raster corrupted.",
                FreeTypeError.RasterOverflow => "Raster overflow.",
                FreeTypeError.RasterNegativeHeight => "Negative height while rastering.",
                FreeTypeError.TooManyCaches => "Too many registered caches.",
                FreeTypeError.InvalidOpCode => "Invalid opcode.",
                FreeTypeError.TooFewArguments => "Too few arguments.",
                FreeTypeError.StackOverflow => "Stack overflow.",
                FreeTypeError.CodeOverflow => "Code overflow.",
                FreeTypeError.BadArgument => "Bad argument.",
                FreeTypeError.DivideByZero => "Division by zero.",
                FreeTypeError.InvalidReference => "Invalid reference.",
                FreeTypeError.DebugOpCode => "Found debug opcode.",
                FreeTypeError.EndfInExecStream => "Found ENDF opcode in execution stream.",
                FreeTypeError.NestedDefs => "Nested DEFS.",
                FreeTypeError.InvalidCodeRange => "Invalid code range.",
                FreeTypeError.ExecutionTooLong => "Execution context too long.",
                FreeTypeError.TooManyFunctionDefs => "Too many function definitions.",
                FreeTypeError.TooManyInstructionDefs => "Too many instruction definitions.",
                FreeTypeError.TableMissing => "SFNT font table missing.",
                FreeTypeError.HorizHeaderMissing => "Horizontal header (hhea) table missing.",
                FreeTypeError.LocationsMissing => "Locations (loca) table missing.",
                FreeTypeError.NameTableMissing => "Name table missing.",
                FreeTypeError.CMapTableMissing => "Character map (cmap) table missing.",
                FreeTypeError.HmtxTableMissing => "Horizontal metrics (hmtx) table missing.",
                FreeTypeError.PostTableMissing => "PostScript (post) table missing.",
                FreeTypeError.InvalidHorizMetrics => "Invalid horizontal metrics.",
                FreeTypeError.InvalidCharMapFormat => "Invalid character map (cmap) format.",
                FreeTypeError.InvalidPPem => "Invalid ppem value.",
                FreeTypeError.InvalidVertMetrics => "Invalid vertical metrics.",
                FreeTypeError.CouldNotFindContext => "Could not find context.",
                FreeTypeError.InvalidPostTableFormat => "Invalid PostScript (post) table format.",
                FreeTypeError.InvalidPostTable => "Invalid PostScript (post) table.",
                FreeTypeError.SyntaxError => "Opcode syntax error.",
                FreeTypeError.StackUnderflow => "Argument stack underflow.",
                FreeTypeError.Ignore => "Ignore this error.",
                FreeTypeError.NoUnicodeGlyphName => "No Unicode glyph name found.",
                FreeTypeError.MissingStartfontField => "`STARTFONT' field missing.",
                FreeTypeError.MissingFontField => "`FONT' field missing.",
                FreeTypeError.MissingSizeField => "`SIZE' field missing.",
                FreeTypeError.MissingFontboudingboxField => "`FONTBOUNDINGBOX' field missing.",
                FreeTypeError.MissingCharsField => "`CHARS' field missing.",
                FreeTypeError.MissingStartcharField => "`STARTCHAR' field missing.",
                FreeTypeError.MissingEncodingField => "`ENCODING' field missing.",
                FreeTypeError.MissingBbxField => "`BBX' field missing.",
                FreeTypeError.BbxTooBig => "`BBX' too big.",
                FreeTypeError.CorruptedFontHeader => "Font header corrupted or missing fields.",
                FreeTypeError.CorruptedFontGlyphs => "Font glyphs corrupted or missing fields.",
                _ => throw new ArgumentOutOfRangeException(nameof(error))
            };
        }
    }
}
