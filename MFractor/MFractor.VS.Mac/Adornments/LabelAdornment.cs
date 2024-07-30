using System;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.IOC;
using MFractor.VS.Mac.Views;

namespace MFractor.VS.Mac.Adornments
{
    public class LabelAdornment : NSTextView
    {
        public LabelAdornment() : base()
        {
            SetData(String.Empty);
            Build();
        }
        string text;

        public LabelAdornment(string text) : base()
        {
            SetData(text);
            Build();
        }

        void Build()
        {
            Editable = false;
            //this.Font = NSFont.SystemFontOfSize(10.0f);

            Alignment = NSTextAlignment.Center;
            BackgroundColor = NSColor.Clear;

            var theme = Resolver.Resolve<IThemeService>();

            if (theme.CurrentTheme == Theme.Dark)
            {
                TextColor = NSColor.White;
            }
        }

        public LabelAdornment(IntPtr handle) : base(handle)
        {
            SetData(text);
            Build();
        }

        [Export("initWithFrame:")]
        public LabelAdornment(CGRect frameRect) : base(frameRect)
        {
            SetData(text);
            Build();
        }

        public void SetData(string text)
        {
            this.text = text;

            if (Value != this.text)
            {
                Value = this.text;

                var frameSize = Value.GetTextSize(this.Font);

                SetFrameSize(new CGSize(frameSize.Width * 2, frameSize.Height * 1.5));
            }
        }
    }
}
