using System;
using MFractor.Fonts;

namespace MFractor.Editor.Tooltips
{
    public class FontPreviewTooltipModel
    {
        public FontPreviewTooltipModel(IFont font,
                                       string previewContent = "Lorem Ipsum")
        {
            if (string.IsNullOrEmpty(previewContent))
            {
                throw new ArgumentException("message", nameof(previewContent));
            }

            Font = font;
            PreviewContent = previewContent.Replace("&#10;", Environment.NewLine)
                                            .Replace("&gt;", ">")
                                            .Replace("&lt;", "<")
                                            .Replace("&amp;", "&")
                                            .Replace("&quot;", "\"");
        }

        public IFont Font { get; set; }

        public string PreviewContent { get; }
    }
}
