using System;
using System.ComponentModel.Composition;

namespace MFractor.Fonts.Tooltips
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontSummaryTooltipRenderer))]
    class FontSummaryTooltipRenderer : IFontSummaryTooltipRenderer
    {
        public string Render(IFont font)
        {
            if (font is null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            return font.FileName
                   + "\nName: " + font.Name
                   + "\nStyle: " + font.Style
                   + "\nPostScript Name: " + font.PostscriptName
                   + "\nFamily: " + font.FamilyName;
        }
    }
}