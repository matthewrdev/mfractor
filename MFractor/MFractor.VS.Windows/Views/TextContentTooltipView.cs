using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using MFractor.Analytics;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Editor.Tooltips;
using MFractor.IOC;
using MFractor.Navigation;
using MFractor.VS.Windows.Utilities;
using MFractor.Work;

namespace MFractor.VS.Windows.Views
{
    public class TextContentTooltipView : StackPanel
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        Label summaryLabel;

        public string Content => TooltipModel?.Content;
        public INavigationSuggestion NavigationSuggestion => TooltipModel?.NavigationSuggestion;
        public INavigationContext NavigationContext => TooltipModel?.NavigationContext;
        public InteractionLocation InteractionLocation => TooltipModel?.InteractionLocation;
        public IFeatureContext FeatureContext => TooltipModel?.FeatureContext;
        public IReadOnlyList<ICodeAction> CodeActions => TooltipModel?.CodeActions;
        public string HelpUrl => TooltipModel?.HelpUrl;

        public TextContentTooltipModel TooltipModel { get; private set; }

        public TextContentTooltipView()
        {
            Orientation = Orientation.Vertical;
            Initialise(TooltipModel);
        }

        public TextContentTooltipView(TextContentTooltipModel tooltipModel)
        {
            Initialise(tooltipModel);
        }

        public void Initialise(TextContentTooltipModel tooltipModel)
        {
            try
            {
                TooltipModel = tooltipModel;

                BuildTextContent();

                BuildNavigationLabel();

                BuildCodeActionSuggestions();

                BuildFooter();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
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

        void BuildCodeActionSuggestions()
        {
            if (FeatureContext != null && CodeActions != null)
            {
                foreach (var codeAction in CodeActions)
                {
                    var suggestions = codeAction.Suggest(FeatureContext, InteractionLocation);

                    if (suggestions != null && suggestions.Any())
                    {
                        foreach (var suggestion in suggestions)
                        {
                            var actionButton = new ClickableLabel(suggestion.Description, async () =>
                            {
                                var result = codeAction.Execute(FeatureContext, suggestion, InteractionLocation);

                                if (result != null && result.Any())
                                {
                                    Resolver.Resolve<IAnalyticsService>().Track(codeAction);
                                    await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                                }
                            });

                            Children.Add(actionButton);
                        }
                    }
                }
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

        private void BuildFooter()
        {
            Children.Add(new Separator());

            Children.Add(new BrandedFooter(HelpUrl));
        }
    }
}
