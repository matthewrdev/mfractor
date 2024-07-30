using System;
using System.ComponentModel.Composition;
using System.IO;
using AppKit;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.Fonts;
using MFractor.Fonts.Rendering;
using MFractor.VS.Mac.Views;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Mac.Tooltips
{
    [Export(typeof(IViewElementFactory))]
    [Name("MFractor FontPreviewTooltipModel to UIElement")]
    [TypeConversion(from: typeof(FontPreviewTooltipModel), to: typeof(NSView))]
    [Order(Before = "default")]
    class FontPreviewTooltipViewElementFactory : IViewElementFactory
    {
        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        [ImportingConstructor]
        public FontPreviewTooltipViewElementFactory(Lazy<IAnalyticsService> analyticsService,
                                                    Lazy<IFontService> fontService)
        {
            this.analyticsService = analyticsService;
            this.fontService = fontService;
        }

        public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
        {
            // Should never happen if the service's code is correct, but it's good to be paranoid.
            if (typeof(TView) != typeof(NSView) || !(model is FontPreviewTooltipModel tooltipModel))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            AnalyticsService.Track("Font Asset Tooltip");

            return new FontTooltipView(tooltipModel.Font, tooltipModel.PreviewContent, false) as TView;
        }
    }
}