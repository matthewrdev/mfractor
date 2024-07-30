using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.IOC;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.VS.Mac.Adornments
{
    class ColorAdornment : NSView
    {
        public ColorAdornment() : base()
        {
            Build();
            Update(Color, ColorEditedDelegate, true);
        }

        public ColorAdornment(Color color, ColorEditedDelegate colorEditedDelegate) : base()
        {
            Build();
            Update(color, colorEditedDelegate, true);
        }

        public ColorAdornment(IntPtr handle) : base(handle)
        {
            Build();
            Update(Color, ColorEditedDelegate, true);
        }

        [Export("initWithFrame:")]
        public ColorAdornment(CGRect frameRect) : base(frameRect)
        {
            Build();
            Update(Color, ColorEditedDelegate, true);
        }

        void Build()
        {
            WantsLayer = true;
            var theme = Resolver.Resolve<IThemeService>();

            if (theme.CurrentTheme == Theme.Dark)
            {
                Layer.BorderColor = NSColor.White.CGColor;
            }
            else
            {
                Layer.BorderColor = NSColor.Black.CGColor;
            }

            Layer.BorderWidth = 2;

            SetFrameSize(new CGSize(15, 15));
        }

        public Color Color { get; private set; } = Color.WhiteSmoke;

        public ColorEditedDelegate ColorEditedDelegate { get; set; }

        internal void Update(Color color, ColorEditedDelegate colorEditedDelegate, bool force = false)
        {
            ColorEditedDelegate = colorEditedDelegate;
            if (this.Color != color || force)
            {
                this.Color = color;

                Layer.BackgroundColor = NSColor.FromRgb(color.R, color.G, color.B).CGColor;
                NeedsLayout = true;
                NeedsDisplay = true;
            }
        }

        public override void MouseUp(NSEvent theEvent)
        {
            base.MouseUp(theEvent);

            if (ColorEditedDelegate != null)
            {
                Resolver.Resolve<IWorkEngine>().ApplyAsync(new ColorEditorWorkUnit(Color, ColorEditedDelegate));
            }
        }
    }
}