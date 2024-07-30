using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using MFractor.Utilities;

namespace MFractor.Fonts.Rendering
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontRenderingPaths))]
    class FontRenderingPaths : IFontRenderingPaths, IApplicationLifecycleHandler
    {
        readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        public string FontRenderFolder => Path.Combine(ApplicationPaths.ApplicationDataPath, "font-renders");

        [ImportingConstructor]
        public FontRenderingPaths(Lazy<IApplicationPaths> applicationPaths)
        {
            this.applicationPaths = applicationPaths;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            if (!Directory.Exists(FontRenderFolder))
            {
                Directory.CreateDirectory(FontRenderFolder);
            }
        }

        public string GetPreviewPath(IFont font)
        {
            if (font is null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            var assetName = CSharpNameHelper.ToDotNetName(font.PostscriptName);

            return Path.Combine(FontRenderFolder, assetName);
        }

        public string GetGlyphPreviewPath(IFont font, string glyphCode, Color color, int width, int height)
        {
            if (font is null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            if (string.IsNullOrEmpty(glyphCode))
            {
                throw new ArgumentException("message", nameof(glyphCode));
            }

            var encoded = Base64Helper.Encode(glyphCode);

            var size = $"{width}x{height}";

            var hex = ColorHelper.GetHexString(color, false);

            var fontName = CSharpNameHelper.ToDotNetName(font.PostscriptName);

            var assetName = fontName + "-" + encoded + hex + size + ".png";

            assetName = assetName.Replace("\\", string.Empty).Replace("/", string.Empty);

            return Path.Combine(FontRenderFolder, assetName);
        }

        public string GetTextPreviewPath(IFont font, string previewText, Color color, int width, int height)
        {
            if (font is null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            if (string.IsNullOrEmpty(previewText))
            {
                throw new ArgumentException("message", nameof(previewText));
            }

            var encoded = SHA1Helper.FromString(previewText);

            var size = $"{width}x{height}";

            var hex = ColorHelper.GetHexString(color, false);

            var fontName = CSharpNameHelper.ToDotNetName(font.PostscriptName);

            var assetName = fontName + "-" + encoded + hex + size + ".png";

            assetName = assetName.Replace("\\", string.Empty).Replace("/", string.Empty);

            return Path.Combine(FontRenderFolder, assetName);
        }
    }
}