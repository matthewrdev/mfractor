using System;
using System.ComponentModel.Composition;
using System.Drawing;
using AppKit;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.Work.WorkUnits;
using MFractor.VS.Mac.Views;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using MFractor.Work;

namespace MFractor.VS.Mac.Tooltips
{
    [Export(typeof(IViewElementFactory))]
    [Name("MFractor ColorTooltipModel to UIElement")]
    [TypeConversion(from: typeof(ColorTooltipModel), to: typeof(NSView))]
    [Order(Before = "default")]
    class ColorTooltipViewElementFactory : IViewElementFactory
    {
        readonly Lazy<IWorkEngine> workUnitEngine;
        public IWorkEngine  WorkUnitEngine => workUnitEngine.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        [ImportingConstructor]
        public ColorTooltipViewElementFactory(Lazy<IWorkEngine> workUnitEngine,
                                              Lazy<IAnalyticsService> analyticsService)
        {
            this.workUnitEngine = workUnitEngine;
            this.analyticsService = analyticsService;
        }

        public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
        {
            // Should never happen if the service's code is correct, but it's good to be paranoid.
            if (typeof(TView) != typeof(NSView) || !(model is ColorTooltipModel tooltipModel))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            var color = tooltipModel.Color;

            var view = new ColorTooltipView(color, tooltipModel.NavigationContext, tooltipModel.NavigationSuggestion, tooltipModel.Label);
            view.ColorClicked += (sender, args) =>
            {
                AnalyticsService.Track("Copied Color To Clipboard (Tooltip)");
                WorkUnitEngine.ApplyAsync(new CopyValueToClipboardWorkUnit(GetHexString(color), $"Copied {GetHexString(color)} to clipboard"));
            };

            return view as TView;
        }

        public string GetHexString(Color color)
        {
            return color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
    }
}