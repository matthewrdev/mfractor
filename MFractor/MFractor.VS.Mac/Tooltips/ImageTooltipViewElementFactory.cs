using System;
using System.ComponentModel.Composition;
using AppKit;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.Images.WorkUnits;
using MFractor.VS.Mac.Views;
using MFractor.Work;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Mac.Tooltips
{
    [Export(typeof(IViewElementFactory))]
	[Name("MFractor ImageTooltipModel to UIElement")]
	[TypeConversion(from: typeof(ImageTooltipModel), to: typeof(NSView))]
	[Order(Before = "default")]
	sealed class ImageTooltipViewElementFactory : IViewElementFactory
	{
		readonly Lazy<IImageUtilities> imageUtil;
        public IImageUtilities ImageUtil => imageUtil.Value;

		readonly Lazy<IWorkEngine> workUnitEngine;
        public IWorkEngine  WorkUnitEngine => workUnitEngine.Value;

		readonly Lazy<IAnalyticsService> analyticsService;
		public IAnalyticsService AnalyticsService => analyticsService.Value;

		[ImportingConstructor]
        public ImageTooltipViewElementFactory(Lazy<IImageUtilities> imageUtil,
                                              Lazy<IWorkEngine> workUnitEngine,
                                              Lazy<IAnalyticsService> analyticsService)
        {
            this.imageUtil = imageUtil;
            this.workUnitEngine = workUnitEngine;
            this.analyticsService = analyticsService;
        }

		public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
		{
			// Should never happen if the service's code is correct, but it's good to be paranoid.
			if (typeof(TView) != typeof(NSView) || !(model is ImageTooltipModel tooltipModel))
			{
				throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
			}

			var imageAsset = tooltipModel.ImageAsset;

            if (imageAsset == null)
            {
				return default;
            }

			var imageSize = ImageUtil.GetImageSize(imageAsset.PreviewImageFilePath);

			var imageTooltip = new ImageAssetTooltipView(imageAsset.PreviewImageFilePath, imageSize);
			imageTooltip.Clicked += (sender, args) =>
			{
				AnalyticsService.Track("Go To Image Asset (Tooltip)");
				WorkUnitEngine.ApplyAsync(new ViewImageAssetWorkUnit(imageAsset));
			};

			return imageTooltip as TView;
		}
	}
}