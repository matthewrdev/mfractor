using System;

namespace MFractor.Fonts.Tooltips
{
    public interface IFontSummaryTooltipRenderer
    {
        string Render(IFont font);
    }
}