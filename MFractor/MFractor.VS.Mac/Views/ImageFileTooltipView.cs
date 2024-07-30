using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.Images;
using MFractor.Utilities;
using MFractor.Views.Branding;

namespace MFractor.VS.Mac.Views
{
    /// <summary>
    /// An MFractor branded image tooltip.
    /// </summary>
    public class ImageFileTooltipView : NSStackView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        NSImageView imageView;

        public string ImageFilePath { get; private set; }
        public Size ImageSize { get; private set; }

        public ImageFileTooltipView()
        {
            Initialise();
            SetImage(ImageFilePath, ImageSize);
        }

        public ImageFileTooltipView(string imageFilePath, Size imageSize)
        {
            Initialise();
            SetImage(imageFilePath, imageSize);
        }

        public ImageFileTooltipView(IntPtr handle) : base(handle)
        {
            Initialise();
            SetImage(ImageFilePath, ImageSize);
        }

        [Export("initWithFrame:")]
        public ImageFileTooltipView(CGRect frameRect) : base(frameRect)
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

        public void SetImage(string imageFilePath, Size imageSize)
        {
            try
            {
                ImageFilePath = imageFilePath;
                ImageSize = imageSize;

                if (imageFilePath != null && imageSize.Height > 0)
                {
                    var image = new NSImage(imageFilePath, true)
                    {
                        Size = new CoreGraphics.CGSize(imageSize.Width, imageSize.Height),
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
    }
}
