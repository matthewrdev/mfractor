using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.IOC;
using MFractor.Navigation;
using MFractor.VS.Windows.Utilities;
using MFractor.VS.Windows.Views;
using MFractor.Work;

namespace MFractor.VS.Windows
{
    public class ThicknessPreviewView : StackPanel
    {
        readonly Logging.ILogger log = Logging.Logger.Create();
        private BrandedFooter footer;

        private StackPanel middleStackView;

        private Frame contentBox;

        private Label topThicknessValueLabel;
        private Label bottomThicknessValueLabel;
        private Label rightThicknessValueLabel;
        private Label leftThicknessValueLabel;
        private Label summaryLabel;

        public string TopValue => TooltipModel?.Top.ToString() ?? "NA";
        public string LeftValue => TooltipModel?.Left.ToString() ?? "NA";
        public string RightValue => TooltipModel?.Right.ToString() ?? "NA";
        public string BottomValue => TooltipModel?.Bottom.ToString() ?? "NA";
        public string Content => TooltipModel?.Content;

        public INavigationSuggestion NavigationSuggestion => TooltipModel?.NavigationSuggestion;
        public INavigationContext NavigationContext => TooltipModel?.NavigationContext;

        public ThicknessPreviewView() : base()
        {
            Orientation = Orientation.Vertical;
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
                Orientation = Orientation.Vertical;
                TooltipModel = tooltipModel;

                BuildTextContent();

                BuildTop();

                BuildMiddle();

                BuildBottom();

                BuildNavigationLabel();

                BuildFooter();

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
        private void BuildTextContent()
        {
            if (!string.IsNullOrEmpty(Content))
            {
                summaryLabel = new Label()
                {
                    Foreground = TextColorHelper.GetThemeTextBrush(),
                    Content = Content,
                };
                Children.Add(summaryLabel);
            }
        }

        private void BuildNavigationLabel()
        {
            if (NavigationSuggestion != null && NavigationContext != null)
            {
                var fixLabel = new ClickableLabel(NavigationSuggestion.Label, async () =>
                {
                    var result = await Resolver.Resolve<INavigationService>().Navigate(NavigationContext, NavigationSuggestion);

                    if (result != null && result.Any())
                    {
                        Resolver.Resolve<IAnalyticsService>().Track(NavigationSuggestion.Label);
                        await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                    }
                });

                Children.Add(fixLabel);
            }
        }

        private void BuildBottom()
        {
            bottomThicknessValueLabel = new Label()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                Foreground = TextColorHelper.GetThemeTextBrush(),
                Content = BottomValue,
            };

            Children.Add(bottomThicknessValueLabel);
        }

        private void BuildMiddle()
        {
            var middleContainer = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
            };

            leftThicknessValueLabel = new Label()
            {
                Foreground = TextColorHelper.GetThemeTextBrush(),
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Content = LeftValue,
            };

            middleContainer.Children.Add(leftThicknessValueLabel);

            contentBox = new Frame()
            {
                BorderBrush = TextColorHelper.GetThemeTextBrush(),
              BorderThickness = new System.Windows.Thickness(2),
            };

            contentBox.Width = 30;
            contentBox.Height = 30;
            middleContainer.Children.Add(contentBox);

            rightThicknessValueLabel = new Label()
            {
                Foreground = TextColorHelper.GetThemeTextBrush(),
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Content = RightValue,
            };

            middleContainer.Children.Add(rightThicknessValueLabel);

            Children.Add(middleContainer);
        }

        private void BuildTop()
        {
            topThicknessValueLabel = new Label()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                Foreground = TextColorHelper.GetThemeTextBrush(),
                Content = TopValue ?? string.Empty,
            };

            Children.Add(topThicknessValueLabel);
        }

        private void BuildFooter()
        {
            Children.Add(new Separator());

            Children.Add(new BrandedFooter());
        }
    }
}
