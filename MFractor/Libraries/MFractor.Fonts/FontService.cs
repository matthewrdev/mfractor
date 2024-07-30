using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Fonts.Utilities;
using Typography.OpenFont;

namespace MFractor.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontService))]
    class FontService : IFontService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IGlyphResolver> glyphResolver;
        public IGlyphResolver GlyphResolver => glyphResolver.Value;

        [ImportingConstructor]
        public FontService(Lazy<IGlyphResolver> glyphResolver)
        {
            this.glyphResolver = glyphResolver;
        }

        public IFont GetFont(string fontFilePath)
        {
            if (!IsFontFile(fontFilePath))
            {
                return default;
            }

            if (!File.Exists(fontFilePath))
            {
                return default;
            }

            try
            {
                PreviewFontInfo info = null;

                using (var fs = File.OpenRead(fontFilePath))
                {
                    info = new OpenFontReader().ReadPreview(fs);
                }

                if (info == null)
                {
                    return default;
                }

                return new Font(fontFilePath,
                                info.Name,
                                info.FullName,
                                info.SubFamilyName,
                                info.PostscriptName,
                                info.TypographicFamilyName,
                                info.IsWebFont);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return default;
        }

        public bool IsFontFile(string fontFilePath)
        {
            return FontAssetHelper.IsFontAsset(fontFilePath);
        }

        public IFontTypeface GetFontTypeface(string fontFilePath)
        {
            var fontInfo = GetFont(fontFilePath);
            if (fontInfo == null)
            {
                return null;
            }

            return new FontTypeface(fontInfo, GlyphResolver);
        }

        public IFontTypeface GetFontTypeface(IFont font)
        {
            if (font is null)
            {
                return null;
            }

            var fontFilePath = font.FilePath;

            return GetFontTypeface(fontFilePath);
        }
    }
}