using System;
using System.ComponentModel.Composition;
using MFractor.Editor.Tooltips;
using MFractor.VS.Mac.Views;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Mac.Tooltips
{
    [Export(typeof(IViewElementFactory))]
	[Name("MFractor ThicknessTooltipModel to UIElement")]
	[TypeConversion(from: typeof(ThicknessTooltipModel), to: typeof(AppKit.NSView))]
	[Order(After = "default")]
	sealed class ThicknessTooltipViewElementFactory : IViewElementFactory
	{
		readonly Logging.ILogger log = Logging.Logger.Create();

		public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
		{
			// Should never happen if the service's code is correct, but it's good to be paranoid.
			if (typeof(TView) != typeof(AppKit.NSView) || !(model is ThicknessTooltipModel tooltipModel))
			{
				throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
			}

			var view = new ThicknessPreviewView(tooltipModel);

			return view as TView;
		}
	}
}