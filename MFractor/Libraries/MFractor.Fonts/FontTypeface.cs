using System;

namespace MFractor.Fonts
{
    class FontTypeface : IFontTypeface
    {
        public IFont FontInformation { get; }

        readonly Lazy<IGlyphCollection> glyphCollection;
        public IGlyphCollection GlyphCollection => glyphCollection.Value;

        public FontTypeface(IFont fontInformation,
                            IGlyphResolver glyphResolver)
        {
            FontInformation = fontInformation ?? throw new System.ArgumentNullException(nameof(fontInformation));

            glyphCollection = new Lazy<IGlyphCollection>(() => glyphResolver.GetGlyphs(fontInformation));
        }
    }
}