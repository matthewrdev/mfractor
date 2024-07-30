using System;
using System.Drawing;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.Analytics;
using MFractor.IOC;
using MFractor.Navigation;
using MFractor.Work;

namespace MFractor.VS.Mac.Views
{
    public class ColorTooltipView : NSStackView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        NSTextField summaryLabel;
        ClickableLabel colorLabel;

        public Color Color { get; private set; }
        public string Label { get; private set; }
        public INavigationContext NavigationContext { get; private set; }
        public INavigationSuggestion NavigationSuggestion { get; private set; }

        public event EventHandler ColorClicked;

        public ColorTooltipView(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion)
        {
            Initialise(navigationContext, navigationSuggestion);
            SetColor(Color, Label);
        }

        public ColorTooltipView(Color color,
                                INavigationContext navigationContext,
                                INavigationSuggestion navigationSuggestion,
                                string label = "")
        {
            Initialise(navigationContext, navigationSuggestion);
            SetColor(color, label);
        }

        public ColorTooltipView(IntPtr handle) : base(handle)
        {
            Initialise(NavigationContext, NavigationSuggestion);
            SetColor(Color, Label);
        }

        [Export("initWithFrame:")]
        public ColorTooltipView(CGRect frameRect) : base(frameRect)
        {
            Initialise(NavigationContext, NavigationSuggestion);
            SetColor(Color, Label);
        }

        void Initialise(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion)
        {
            NavigationContext = navigationContext;
            NavigationSuggestion = navigationSuggestion;

            try
            {
                this.ClearChildren();

                Orientation = NSUserInterfaceLayoutOrientation.Vertical;

                summaryLabel = new NSTextField()
                {
                    Editable = false,
                    Selectable = false,
                    Bezeled = false,
                    DrawsBackground = false,
                    StringValue = "",
                    Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                };
                InsertView(summaryLabel, (uint)Views.Length, NSStackViewGravity.Top);

                colorLabel = new ClickableLabel()
                {
                    Editable = false,
                    Selectable = false,
                    Bezeled = false,
                    Alignment = NSTextAlignment.Center,
                    Font = NSFont.SystemFontOfSize(NSFont.LabelFontSize * 2.2f),
                    OnClickedAction = () => ColorClicked?.Invoke(this, EventArgs.Empty)
                };
                InsertView(colorLabel, (uint)Views.Length, NSStackViewGravity.Top);

                if (navigationSuggestion != null && navigationContext != null)
                {
                    var value = new NSMutableAttributedString(navigationSuggestion.Label);
                    var range = new NSRange(0, value.Length);

                    value.AddAttribute(NSStringAttributeKey.ForegroundColor, MonoDevelop.Ide.Gui.Styles.LinkForegroundColor.ToNSColor(), range);
                    value.AddAttribute(NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);

                    var navigationButton = AppKitHelper.CreateLinkButton(navigationSuggestion.Label, async () =>
                    {
                        var result = await Resolver.Resolve<INavigationService>().Navigate(NavigationContext, NavigationSuggestion);

                        if (result != null && result.Any())
                        {
                            Resolver.Resolve<IAnalyticsService>().Track(NavigationSuggestion.Label);
                            await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                        }
                    });

                    InsertView(navigationButton, (uint)Views.Length, NSStackViewGravity.Top);
                }

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

        public void SetColor(Color color,
                             string label = "")
        {
            try
            {
                Color = color;
                Label = label;

                if (!string.IsNullOrEmpty(label))
                {
                    summaryLabel.StringValue = label;
                }

                colorLabel.StringValue = GetHexString(color);
                colorLabel.BackgroundColor = color.ToNSColor();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public string GetHexString(Color color)
        {
            return color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public string GetRGBAString(Color color)
        {
            return color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString();
        }
    }
}
