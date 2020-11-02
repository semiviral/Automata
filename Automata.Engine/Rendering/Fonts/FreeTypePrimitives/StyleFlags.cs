using System;

namespace Automata.Engine.Rendering.Fonts.FreeTypePrimitives
{
    /// <summary>
    ///     A list of bit-flags used to indicate the style of a given face. These are used in the ‘style_flags’ field of
    ///     <see cref="FontFace" />.
    /// </summary>
    /// <remarks>
    ///     The style information as provided by FreeType is very basic. More details are beyond the scope and should be
    ///     done on a higher level (for example, by analyzing various fields of the ‘OS/2’ table in SFNT based fonts).
    ///     Copied from SharpFont (https://github.com/Robmaister/SharpFont)
    /// </remarks>
    [Flags]
    public enum StyleFlags
    {
        /// <summary>No style flags.</summary>
        None = 0x00,

        /// <summary>Indicates that a given face style is italic or oblique.</summary>
        Italic = 0x01,

        /// <summary>Indicates that a given face is bold.</summary>
        Bold = 0x02
    }
}
