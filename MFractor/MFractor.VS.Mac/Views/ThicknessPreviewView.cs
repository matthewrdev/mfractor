using System;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.IOC;
using MFractor.Navigation;
using MFractor.Work;

namespace MFractor.VS.Mac.Views
{
    public class ThicknessPreviewView : NSStackView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();
        private NSBrandedFooter footer;

        private NSStackView thicknessStackView;
        private NSStackView middleStackView;

        private NSBox contentBox;

        private NSTextField topThicknessValueLabel;
        private NSTextField bottomThicknessValueLabel;
        private NSTextField rightThicknessValueLabel;
        private NSTextField leftThicknessValueLabel;
        private NSTextField summaryLabel;

        public string TopValue => TooltipModel?.Top.ToString() ?? "NA";
        public string LeftValue => TooltipModel?.Left.ToString() ?? "NA";
        public string RightValue => TooltipModel?.Right.ToString() ?? "NA";
        public string BottomValue => TooltipModel?.Bottom.ToString() ?? "NA";
        public string Content => TooltipModel?.Content;

        public INavigationSuggestion NavigationSuggestion => TooltipModel?.NavigationSuggestion;
        public INavigationContext NavigationContext => TooltipModel?.NavigationContext;

        public ThicknessPreviewView() : base()
        {
            Initialise(TooltipModel);
        }

        public ThicknessPreviewView(IntPtr handle) : base(handle)
        {
            Initialise(TooltipModel);
        }

        [Export("initWithFrame:")]
        public ThicknessPreviewView(CGRect frameRect) : base(frameRect)
        {
            Initialise(TooltipModel);
        }

        public ThicknessPreviewView(ThicknessTooltipModel tooltipModel)
            : base()
        {
            Initialise(tooltipModel);
        }

        public ThicknessTooltipModel TooltipModel { get; private set; }

        private void Initialise(ThicknessTooltipModel tooltipModel)
        {
            try
            {
                Orientation = NSUserInterfaceLayoutOrientation.Vertical;
                TooltipModel = tooltipModel;

                BuildSummaryLabel();

                BuildThicknessPreview();

                BuildNavigationLabel();

                BuildFooter();

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        private void BuildThicknessPreview()
        {
            thicknessStackView = new NSStackView()
            {
                Orientation = NSUserInterfaceLayoutOrientation.Vertical,
                Alignment = NSLayoutAttribute.CenterX,
            };

            BuildTop();

            BuildMiddle();

            BuildBottom();

            InsertView(thicknessStackView, (uint)Views.Length, NSStackViewGravity.Top);
        }
        private void BuildBottom()
        {
            bottomThicknessValueLabel = new NSTextField()
            {
                Editable = false,
                Selectable = false,
                Bezeled = false,
                DrawsBackground = false,
                LineBreakMode = NSLineBreakMode.ByWordWrapping,
                Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                Alignment = NSTextAlignment.Center,
                StringValue = BottomValue,
            };

            thicknessStackView.InsertView(bottomThicknessValueLabel, (uint)thicknessStackView.Views.Length, NSStackViewGravity.Top);
        }

        private void BuildMiddle()
        {
            var middleContainer = new NSStackView()
            {
                Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
                Alignment = NSLayoutAttribute.CenterY,
            };

            leftThicknessValueLabel = new NSTextField()
            {
                Editable = false,
                Selectable = false,
                Bezeled = false,
                DrawsBackground = false,
                LineBreakMode = NSLineBreakMode.ByWordWrapping,
                Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                Alignment = NSTextAlignment.Center,
                StringValue = LeftValue,
            };

            middleContainer.InsertView(leftThicknessValueLabel, (uint)middleContainer.Views.Length, NSStackViewGravity.Top);

            contentBox = new NSBox()
            {
                BoxType = NSBoxType.NSBoxCustom,
                BorderType = NSBorderType.LineBorder,
                BorderWidth = 2,
                Title = string.Empty,
            };

            contentBox.SetFrameSize(new CGSize(50, 50));
            middleContainer.InsertView(contentBox, (uint)middleContainer.Views.Length, NSStackViewGravity.Top);

            rightThicknessValueLabel = new NSTextField()
            {
                Editable = false,
                Selectable = false,
                Bezeled = false,
                DrawsBackground = false,
                LineBreakMode = NSLineBreakMode.ByWordWrapping,
                Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                Alignment = NSTextAlignment.Center,
                StringValue = RightValue,
            };

            middleContainer.InsertView(rightThicknessValueLabel, (uint)middleContainer.Views.Length, NSStackViewGravity.Top);

            thicknessStackView.InsertView(middleContainer, (uint)thicknessStackView.Views.Length, NSStackViewGravity.Top);
        }

        private void BuildTop()
        {
            topThicknessValueLabel = new NSTextField()
            {
                Editable = false,
                Selectable = false,
                Bezeled = false,
                DrawsBackground = false,
                LineBreakMode = NSLineBreakMode.ByWordWrapping,
                Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                Alignment = NSTextAlignment.Center,
                StringValue = TopValue ?? string.Empty,
            };

            thicknessStackView.InsertView(topThicknessValueLabel, (uint)thicknessStackView.Views.Length, NSStackViewGravity.Top);
        }

        private void BuildSummaryLabel()
        {
            if (!string.IsNullOrEmpty(Content))
            {
                summaryLabel = new NSTextField()
                {
                    Editable = false,
                    Selectable = false,
                    Bezeled = false,
                    DrawsBackground = false,
                    LineBreakMode = NSLineBreakMode.ByWordWrapping,
                    Font = NSFont.SystemFontOfSize(NSFont.SmallSystemFontSize),
                    StringValue = Content ?? string.Empty,
                };
                InsertView(summaryLabel, (uint)Views.Length, NSStackViewGravity.Top);
            }
        }

        private void BuildNavigationLabel()
        {
            if (NavigationSuggestion != null
                && NavigationContext != null
                && !string.IsNullOrEmpty(NavigationSuggestion.Label))
            {
                var navigationButton = AppKitHelper.CreateLinkButton(NavigationSuggestion.Label, async () =>
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
        }


        void BuildFooter()
        {
            InsertView(new NSBox()
            {
                BoxType = NSBoxType.NSBoxSeparator,

            }, (uint)Views.Length, NSStackViewGravity.Top);

            footer = new NSBrandedFooter();

            InsertView(footer, (uint)Views.Length, NSStackViewGravity.Top);
        }

    }
}
