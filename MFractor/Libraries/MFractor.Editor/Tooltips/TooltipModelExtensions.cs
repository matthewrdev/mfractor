using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Analytics;
using MFractor.IOC;
using MFractor.Navigation;
using MFractor.Work;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;

namespace MFractor.Editor.Tooltips
{
    public static class TooltipModelExtensions
    {
        public static ClassifiedTextElement AsTextElement(this TextContentTooltipModel textContent)
        {
            if (textContent is null)
            {
                throw new ArgumentNullException(nameof(textContent));
            }

            var textRuns = new List<ClassifiedTextRun>
            {
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, "MFractor\n" + textContent.Content + "\n")
            };

            if (textContent.NavigationSuggestion != null
                && textContent.NavigationContext != null
                && !string.IsNullOrWhiteSpace(textContent.NavigationSuggestion.Label))
            {
                async void executeCodeAction()
                {
                    var result = await Resolver.Resolve<INavigationService>().Navigate(textContent.NavigationContext, textContent.NavigationSuggestion);

                    if (result != null && result.Any())
                    {
                        Resolver.Resolve<IAnalyticsService>().Track(textContent.NavigationSuggestion.Label);
                        await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                    }
                }

                var action = new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, textContent.NavigationSuggestion.Label + "\n", executeCodeAction, tooltip: null, ClassifiedTextRunStyle.Underline);

                textRuns.Add(action);
            }

            if (textContent.CodeActions != null
                && textContent.CodeActions.Any())
            {
                textRuns.AddRange(BuildCodeActions(textContent));
            }

            return new ClassifiedTextElement(textRuns);
        }

        private static IReadOnlyList<ClassifiedTextRun> BuildCodeActions(TextContentTooltipModel textContent)
        {
            var featureContext = textContent.FeatureContext;
            var codeActions = textContent.CodeActions;
            var interactionLocation = textContent.InteractionLocation;

            var runs = new List<ClassifiedTextRun>();

            if (featureContext != null && codeActions != null)
            {
                foreach (var codeAction in codeActions)
                {
                    var suggestions = codeAction.Suggest(featureContext, interactionLocation);

                    if (suggestions != null && suggestions.Any())
                    {
                        foreach (var suggestion in suggestions)
                        {
                            async void executeCodeAction()
                            {
                                var result = codeAction.Execute(featureContext, suggestion, interactionLocation);

                                if (result != null && result.Any())
                                {
                                    Resolver.Resolve<IAnalyticsService>().Track(codeAction);
                                    await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                                }
                            }

                            var action = new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, suggestion.Description + "\n", executeCodeAction, tooltip: null, ClassifiedTextRunStyle.Underline);

                            runs.Add(action);
                        }
                    }
                }
            }

            return runs;
        }
    }
}
