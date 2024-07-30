using System;
using System.Collections.Generic;

namespace MFractor.Fonts
{
    public interface IGlyphCollection : IEnumerable<IFontGlyph>
    {
        IFontGlyph GetGlyphByName(string name);

        IFontGlyph GetGlyphByCharacterCode(string characterCode);

        IFontGlyph GetGlyphByCodePoint(uint codePoint);

        IReadOnlyList<IFontGlyph> Glyphs { get; }
    }
}