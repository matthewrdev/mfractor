using System;
using System.ComponentModel.Composition;
using System.Windows;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.VS.Windows.Views;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Windows.Tooltips.Fonts
{
    [Export(typeof(IViewElementFactory))]
    [Name("MFractor FontGlyphTooltipModel to UIElement")]
    [TypeConversion(from: typeof(FontGlyphTooltipModel), to: typeof(UIElement))]
    [Order(Before = "default")]
    class FontGlyphTooltipViewElementFactory : IViewElementFactory
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

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
            if (typeof(TView) != typeof(UIElement) || !(model is FontGlyphTooltipModel tooltipModel))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            var view = new FontGlyphTooltipView();
            view.SetFontText(tooltipModel.Font, tooltipModel.CharacterCode);

            AnalyticsService.Track("Font Glyph Tooltip");

            return view as TView;
        }
    }
}