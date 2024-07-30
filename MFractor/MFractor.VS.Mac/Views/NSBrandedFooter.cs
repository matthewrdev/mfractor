using System;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.IOC;
using MFractor.Views.Branding;

namespace MFractor.VS.Mac.Views
{
    public class NSBrandedFooter : NSStackView
    {
        NSImageView imageView;
        NSTextField label;
        NSButton helpButton;
        public string HelpUrl { get; }

        public NSBrandedFooter()
        {
            Build(HelpUrl);
        }

        public NSBrandedFooter(string helpUrl)
        {
            HelpUrl = helpUrl;
            Build(HelpUrl);
        }

        public NSBrandedFooter(IntPtr handle) : base(handle)
        {
            Build(HelpUrl);
        }

        [Export("initWithFrame:")]
        public NSBrandedFooter(CGRect frameRect) : base(frameRect)
        {
            Build(HelpUrl);
        }

        const string imageResourceId = "MFractor.VS.Mac.Assets.logo-16.png";

        void Build(string helpUrl)
        {
            Orientation = NSUserInterfaceLayoutOrientation.Horizontal;
            var image = new NSImage();

            imageView = new NSImageView();
            imageView.Image = NSImage.FromStream(typeof(NSBrandedFooter).Assembly.GetManifestResourceStream(imageResourceId));
            imageView.Image.Size = new CGSize(8, 8);

            InsertView(imageView, 0, NSStackViewGravity.Leading);

            label = new NSTextField()
            {
                Editable = false,
                Selectable = false,
                Bezeled = false,
                DrawsBackground = false,
                BackgroundColor = NSColor.Clear,
                Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                StringValue = BrandingHelper.BrandingText,
            };

            InsertView(label, (uint)Views.Length, NSStackViewGravity.Top);

            if (!string.IsNullOrEmpty(helpUrl))
            {
                helpButton = AppKitHelper.CreateLinkButton("Help", OnHelpClicked);

                InsertView(helpButton, (uint)Views.Length, NSStackViewGravity.Top);
            }
        }

        void OnHelpClicked()
        {
            Resolver.Resolve<IUrlLauncher>().OpenUrl(HelpUrl);
        }
    }
}