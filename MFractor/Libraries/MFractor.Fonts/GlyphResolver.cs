using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Typography.OpenFont;

namespace MFractor.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IGlyphResolver))]
    class GlyphResolver : IGlyphResolver
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Dictionary<string, IGlyphCollection> fontGlyphCache = new Dictionary<string, IGlyphCollection>();

        public IGlyphCollection GetGlyphs(IFont font, bool useCache = true)
        {
            if (font == null)
            {
                return new GlyphCollection(new Dictionary<string, IFontGlyph>());
            }

            var filePath = font.FilePath;

            if (fontGlyphCache.ContainsKey(filePath))
            {
                if (!useCache)
                {
                    return fontGlyphCache[filePath];
                }
            }

            var glyphs = ReadGlyphsFromFontAsset(filePath);

            fontGlyphCache[filePath] = new GlyphCollection(glyphs);

            return fontGlyphCache[filePath];
        }

        public IReadOnlyDictionary<string, IFontGlyph> ReadGlyphsFromFontAsset(string fontFilePath)
        {
            var glyphs = new Dictionary<string, IFontGlyph>();

            try
            {
                Typeface typeface;
                using (var fs = File.OpenRead(fontFilePath))
                {
                    typeface = new OpenFontReader().Read(fs);
                }

                if (typeface != null)
                {
                    var unicodes = new List<uint>();
                    typeface.CollectUnicode(unicodes);
                    var glyphNameMaps = typeface.GetGlyphNameIter().ToDictionary(gnm => gnm.glyphIndex, gnm => gnm.glyphName);

                    foreach (var unicode in unicodes)
                    {
                        var unicodeHexForm = string.Format("{0:X}", unicode).ToLower();
                        var unicodeInt = Convert.ToInt32(unicodeHexForm, 16);
                        var glyphIndex = typeface.LookupIndex(unicodeInt);

                        if (glyphNameMaps.TryGetValue(glyphIndex, out var name))
                        {
                            // Sanity check, shouldn't happen.
                            if (string.IsNullOrEmpty(name))
                            {
                                continue;
                            }

                            glyphs[name] = new FontGlyph(name, unicodeHexForm, unicode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Info("Failed to read typeface for: " + fontFilePath);
                log?.Exception(ex);
            }

            return glyphs;
        }
    }
}