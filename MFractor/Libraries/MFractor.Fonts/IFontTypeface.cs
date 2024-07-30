using System.Collections.Generic;

namespace MFractor.Fonts
{
    /// <summary>
    /// A font typeface is an <see cref="IFont"/> alongside the <see cref="IFontGlyph"/>'s that the font includes.
    /// <para/>
    /// If the font asset is not a web font, the <see cref="GlyphCollection"/> may be empty.
    /// </summary>
    public interface IFontTypeface
    {
        /// <summary>
        /// The details of this typeface.
        /// <para/>
        /// See the 
        /// </summary>
        IFont FontInformation { get; }

        /// <summary>
        /// The glyphs contained in this font typeface.
        /// <para/>
        /// If the <see cref="FontInformation"/> is not a web font, this collection may be empty.
        /// </summary>
        IGlyphCollection GlyphCollection { get; }
    }
}
