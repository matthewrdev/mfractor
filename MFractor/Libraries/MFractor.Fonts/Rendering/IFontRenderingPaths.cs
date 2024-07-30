using System;
using System.Drawing;

namespace MFractor.Fonts.Rendering
{
    public interface IFontRenderingPaths
    {
        string FontRenderFolder { get; }

        string GetTextPreviewPath(IFont font, string previewText, Color color, int width, int height);

        string GetGlyphPreviewPath(IFont font, string glyphCode, Color color, int width, int height);
    }
}