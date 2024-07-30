using System;

namespace MFractor.Fonts
{
    /// <summary>
    /// Resolves the <see cref="IFontGlyph"/>'s that are available in a <see cref="IFont"/>
    /// <para/>
    /// If a font asset is not a web font, <see cref="GetGlyphs(IFont, bool)"/> may return a null or empty <see cref="IGlyphCollection"/>.
    /// </summary>
    public interface IGlyphResolver
    {
        /// <summary>
        /// For the given <paramref name="font"/>, gets the <see cref="IGlyphCollection"/>.
        /// <para/>
        /// As the glyph resolver may have a precached glyph collection for this font, you can use <paramref name="useCache"/> to force the font to be re-read from disk.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        IGlyphCollection GetGlyphs(IFont font, bool useCache = true);
    }
}