using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.Work.WorkUnits;
using MFractor.VS.Windows.Views;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Xwt;
using MFractor.Work;
using MFractor.VS.Windows.Utilities;

namespace MFractor.VS.Windows.Tooltips.Colors
{
    [Export(typeof(IViewElementFactory))]
    [Name("MFractor ColorTooltipModel to FrameworkElement")]
    [TypeConversion(from: typeof(ColorTooltipModel), to: typeof(UIElement))]
    [Order(Before = "default")]
    class ColorTooltipViewElementFactory : IViewElementFactory
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        [ImportingConstructor]
        public ColorTooltipViewElementFactory(Lazy<IWorkEngine> workEngine,
                                              Lazy<IAnalyticsService> analyticsService)
        {
            this.workEngine = workEngine;
            this.analyticsService = analyticsService;
        }

        public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
        {
            // Should never happen if the service's code is correct, but it's good to be paranoid.
            if (typeof(TView) != typeof(UIElement) || !(model is ColorTooltipModel colorTooltipModel))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            var color = colorTooltipModel.Color;

            var view = new ColorTooltipView(color.ToMediaColor(), colorTooltipModel.NavigationSuggestion, colorTooltipModel.NavigationContext, colorTooltipModel.Label);
            view.ColorClicked += (sender, args) =>
            {
                AnalyticsService.Track("Copied Color To Clipboard (Tooltip)");
                WorkEngine.ApplyAsync(new CopyValueToClipboardWorkUnit(GetHexString(color), $"Copied {GetHexString(color)} to clipboard")).ConfigureAwait(false);
            };

            return view as TView;// Toolkit.CurrentEngine.GetNativeWidget(view) as TView;
        }

        public string GetHexString(Color color)
        {
            return color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

    }
}