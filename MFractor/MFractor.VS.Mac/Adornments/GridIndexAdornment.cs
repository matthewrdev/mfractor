using System;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.IOC;
using MFractor.VS.Mac.Views;

namespace MFractor.VS.Mac.Adornments
{
    public class GridIndexAdornment : NSTextView
    {
        public GridIndexAdornment() : base()
        {
            Build();
        }
        readonly Logging.ILogger log = Logging.Logger.Create();

        public GridIndexAdornment(int index, string sampleCode) : base()
        {
            Build();
            SetData(index, sampleCode);
        }

        void Build()
        {
            Editable = false;
            this.Font = NSFont.SystemFontOfSize(8.0f);

            Alignment = NSTextAlignment.Center;
            BackgroundColor = NSColor.Clear;

            var theme = Resolver.Resolve<IThemeService>();

            if (theme.CurrentTheme == Theme.Dark)
            {
                TextColor = NSColor.White;
            }
        }

        public GridIndexAdornment(IntPtr handle) : base(handle)
        {
            Build();
            SetData(Index, SampleCode);
        }

        [Export("initWithFrame:")]
        public GridIndexAdornment(CGRect frameRect) : base(frameRect)
        {
            Build();
            SetData(Index, SampleCode);
        }

        public override void MouseDown(NSEvent theEvent)
        {
            base.MouseDown(theEvent);

            try
            {
                if (!string.IsNullOrEmpty(SampleCode))
                {
                    Resolver.Resolve<IClipboard>().Text = SampleCode;
                    Resolver.Resolve<IDialogsService>().StatusBarMessage($"Copied {SampleCode} to clipboard");
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public int Index { get; private set; }
        public string SampleCode { get; private set; }

        public void SetData(int index, string sampleCode)
        {
            Index = index;
            SampleCode = sampleCode;

            var newValue = index.ToString();
            if (Value != newValue)
            { 
                this.Value = newValue;

                var frameSize = Value.GetTextSize(this.Font);

                SetFrameSize(new CGSize(frameSize.Width * 3, frameSize.Height * 1.5));
            }
        }
    }
}
