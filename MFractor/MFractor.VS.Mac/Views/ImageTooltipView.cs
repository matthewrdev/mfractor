using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.Utilities;
using MFractor.Views.Branding;

namespace MFractor.VS.Mac.Views
{
    /// <summary>
    /// An MFractor branded image tooltip that allows opening the image asset in the iomage manager by clicking on the image.
    /// </summary>
    public class ImageAssetTooltipView : NSStackView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        NSImageView imageView;

        public string ImageFilePath { get; private set; }
        public ImageSize ImageSize { get; private set; }

        public event EventHandler Clicked;

        public ImageAssetTooltipView()
        {
            Initialise();
            SetImage(ImageFilePath, ImageSize);
        }

        public ImageAssetTooltipView(string imageFilePath, ImageSize imageSize)
        {
            Initialise();
            SetImage(imageFilePath, imageSize);
        }

        public ImageAssetTooltipView(IntPtr handle) : base(handle)
        {
            Initialise();
            SetImage(ImageFilePath, ImageSize);
        }

        [Export("initWithFrame:")]
        public ImageAssetTooltipView(CGRect frameRect) : base(frameRect)
        {
            Initialise();
            SetImage(ImageFilePath, ImageSize);
        }

        void Initialise()
        {
            try
            {
                this.ClearChildren();

                Orientation = NSUserInterfaceLayoutOrientation.Vertical;

                imageView = new NSImageView();

                InsertView(imageView, 0, NSStackViewGravity.Top);

                InsertView(new NSBox()
                {
                    BoxType = NSBoxType.NSBoxSeparator,
                }, 1, NSStackViewGravity.Top);

                InsertView(new NSBrandedFooter(), 2, NSStackViewGravity.Top);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void SetImage(string imageFilePath, ImageSize imageSize)
        {
            try
            {
                ImageFilePath = imageFilePath;
                ImageSize = imageSize;

                if (ImageFilePath != null && imageSize.Height > 0)
                {
                    var resized = ImageHelper.ResizeKeepAspect(imageSize, 128, 128);
                    var image = new NSImage(ImageFilePath, true)
                    {
                        Size = new CoreGraphics.CGSize(resized.Width, resized.Height),
                    };
                    imageView.Image = image;
                }
                else
                {
                    imageView.Image = null;
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public override void MouseUp(NSEvent theEvent)
        {
            base.MouseUp(theEvent);

            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
