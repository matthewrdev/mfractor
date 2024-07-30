using System;
using System.ComponentModel.Composition;
using AppKit;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.VS.Mac.Views;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Mac.Tooltips
{
    [Export(typeof(IViewElementFactory))]
    [Name("MFractor FontGlyphTooltipModel to UIElement")]
    [TypeConversion(from: typeof(FontGlyphTooltipModel), to: typeof(NSView))]
    [Order(Before = "default")]
    class FontGlyphTooltipViewElementFactory : IViewElementFactory
    {
        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        [ImportingConstructor]
        public FontGlyphTooltipViewElementFactory(Lazy<IAnalyticsService> analyticsService)
        {
            this.analyticsService = analyticsService;
        }

        public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
        {
            // Should never happen if the service's code is correct, but it's good to be paranoid.
            if (typeof(TView) != typeof(NSView) || !(model is FontGlyphTooltipModel tooltipModel))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            var font = tooltipModel.Font;
            var characterCode = tooltipModel.CharacterCode;

            AnalyticsService.Track("Font Glyph Tooltip");

            return new FontTooltipView(font, characterCode, true) as TView;
        }
    }
}