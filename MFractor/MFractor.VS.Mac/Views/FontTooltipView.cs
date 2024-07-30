using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using CoreText;
using Foundation;
using MFractor.Fonts;

namespace MFractor.VS.Mac.Views
{
    public class FontTooltipView : NSStackView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        NSTextField label;

        public IFont Font { get; private set; }
        public string Text { get; private set; }
        public bool IsGlyph { get; private set; }

        public FontTooltipView()
        {
            Initialise();
            SetFontDetails(Font, Text, IsGlyph);
        }

        public FontTooltipView(IFont font,
                               string text,
                               bool isGlyph)
        {
            Initialise();
            SetFontDetails(font, text, isGlyph);
        }

        public FontTooltipView(IntPtr handle) : base(handle)
        {
            Initialise();
            SetFontDetails(Font, Text, IsGlyph);
        }

        [Export("initWithFrame:")]
        public FontTooltipView(CGRect frameRect) : base(frameRect)
        {
            Initialise();
            SetFontDetails(Font, Text, IsGlyph);
        }

        void Initialise()
        {
            try
            {
                this.ClearChildren();

                Orientation = NSUserInterfaceLayoutOrientation.Vertical;

                label = new NSTextField()
                {
                    Editable = false,
                    Selectable = false,
                    Bezeled = false,
                    DrawsBackground = false,
                    StringValue = "",
                };
                InsertView(label, (uint)Views.Length, NSStackViewGravity.Top);

                InsertView(new NSBox()
                {
                    BoxType = NSBoxType.NSBoxSeparator,
                }, (uint)Views.Length, NSStackViewGravity.Top);

                InsertView(new NSBrandedFooter(), (uint)Views.Length, NSStackViewGravity.Top);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void SetFontDetails(IFont font,
                                   string text,
                                   bool isGlyph)
        {
            try
            {
                Font = font;
                Text = text;
                IsGlyph = IsGlyph;

                this.label.StringValue = text ?? string.Empty;

                var url = NSUrl.FromFilename(Font.FilePath);
                var data = NSData.FromUrl(url);
                var fontDescriptor = CTFontManager.CreateFontDescriptor(data);
                var size = isGlyph ? NSFont.SystemFontSize * 8 : NSFont.SystemFontSize * 2;
                var ctFont = new CTFont(fontDescriptor, size);
                label.Font = NSFont.FromCTFont(ctFont);

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
