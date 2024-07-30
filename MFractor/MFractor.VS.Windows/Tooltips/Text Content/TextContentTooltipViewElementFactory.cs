using System;
using System.ComponentModel.Composition;
using System.Windows;
using MFractor.Editor.Tooltips;
using MFractor.VS.Windows.Views;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Windows.Tooltips.TextContent
{
    [Export(typeof(IViewElementFactory))]
    [Name("MFractor TextContentTooltipModel to UIElement")]
    [TypeConversion(from: typeof(TextContentTooltipModel), to: typeof(UIElement))]
    [Order(After = "default")]
    sealed class TextContentTooltipViewElementFactory : IViewElementFactory
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
        {
            // Should never happen if the service's code is correct, but it's good to be paranoid.
            if (typeof(TView) != typeof(UIElement) || !(model is TextContentTooltipModel tooltipModel))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            var view = new TextContentTooltipView(tooltipModel);

            return view as TView;
        }
    }
}