using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Fonts
{
    class GlyphCollection : IGlyphCollection
    {
        readonly IReadOnlyDictionary<string, IFontGlyph> glyphs;

        public IReadOnlyList<IFontGlyph> Glyphs => glyphs.Values.ToList();

        readonly Lazy<IReadOnlyDictionary<string, IFontGlyph>> glyphsByCharacterCode;
        IReadOnlyDictionary<string, IFontGlyph> GlyphsByCharacterCode => glyphsByCharacterCode.Value;

        readonly Lazy<IReadOnlyDictionary<uint, IFontGlyph>> glyphsByCodePoint;
        IReadOnlyDictionary<uint, IFontGlyph> GlyphsByCodePoint => glyphsByCodePoint.Value;

        public GlyphCollection(IReadOnlyDictionary<string, IFontGlyph> glyphs)
        {
            this.glyphs = glyphs ?? new Dictionary<string, IFontGlyph>();

            glyphsByCharacterCode = new Lazy<IReadOnlyDictionary<string, IFontGlyph>>(() =>
            {
                return Glyphs.ToDictionary(g => g.CharacterCodeHex, g => g);
            });

            glyphsByCodePoint = new Lazy<IReadOnlyDictionary<uint, IFontGlyph>>(() =>
            {
                return Glyphs.ToDictionary(g => g.Codepoint, g => g);
            });
        }
        public IFontGlyph GetGlyphByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return default;
            }

            if (glyphs.TryGetValue(name, out var glyph))
            {
                return glyph;
            }

            return default;
        }

        public IFontGlyph GetGlyphByCodePoint(uint codePoint)
        {
            if (GlyphsByCodePoint.TryGetValue(codePoint, out var glyph))
            {
                return glyph;
            }

            return default;
        }

        public IFontGlyph GetGlyphByCharacterCode(string characterCode)
        {
            if (string.IsNullOrEmpty(characterCode))
            {
                return default;
            }

            if (GlyphsByCharacterCode.TryGetValue(characterCode, out var glyph))
            {
                return glyph;
            }

            return default;
        }

        public IEnumerator<IFontGlyph> GetEnumerator()
        {
            return Glyphs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Glyphs.GetEnumerator();
        }
    }
}