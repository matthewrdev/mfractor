using System;
using System.ComponentModel.Composition;
using System.Windows;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.Images.WorkUnits;
using MFractor.VS.Windows.Views;
using MFractor.Work;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Windows.Tooltips.Images
{
    [Export(typeof(IViewElementFactory))]
	[Name("MFractor ImageTooltipModel to FrameworkElement")]
	[TypeConversion(from: typeof(ImageTooltipModel), to: typeof(UIElement))]
	[Order(Before = "default")]
	sealed class ImageTooltipViewElementFactory : IViewElementFactory
	{

		readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

		readonly Lazy<IAnalyticsService> analyticsService;
		public IAnalyticsService AnalyticsService => analyticsService.Value;

		[ImportingConstructor]
        public ImageTooltipViewElementFactory(Lazy<IWorkEngine> workEngine,
                                              Lazy<IAnalyticsService> analyticsService)
        {
            this.workEngine = workEngine;
            this.analyticsService = analyticsService;
        }

		public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
		{
			// Should never happen if the service's code is correct, but it's good to be paranoid.
			if (typeof(TView) != typeof(UIElement) || !(model is ImageTooltipModel tooltipModel))
			{
				throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            var imageAsset = tooltipModel.ImageAsset;

            if (imageAsset == null)
            {
				return default;
            }

            var img = System.Drawing.Image.FromFile(imageAsset.PreviewImageFilePath);
            var imageSize = img.Size;
            img.Dispose();

            var view = new ImageFileTooltipView(imageAsset.PreviewImageFilePath, imageSize);
			view.Clicked += (sender, args) =>
			{
				AnalyticsService.Track("Go To Image Asset (Tooltip)");
				WorkEngine.ApplyAsync(new ViewImageAssetWorkUnit(imageAsset)).ConfigureAwait(false);
			};

            return view as TView;
        }
	}
}