using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Editor.Tooltips;
using MFractor.VS.Windows.Utilities;

namespace MFractor.VS.Windows.Views
{
    public class CodeIssueView : StackPanel
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        Label summaryLabel;

        public Color Color { get; }

        public ICodeIssue CodeIssue { get; private set; }
        public IReadOnlyList<ICodeActionSuggestion> Suggestions { get; private set; }
        public Action<CodeIssueFixSelectedEventArgs> CodeFixSelected { get; private set; }

        public string HelpUrl { get; private set; }

        public CodeIssueView(ICodeIssue codeIssue,
                             IReadOnlyList<ICodeActionSuggestion> suggestions,
                             Action<CodeIssueFixSelectedEventArgs> codeFixSelected,
                             string helpUrl)
        {
            Initialise(codeIssue, suggestions, codeFixSelected, helpUrl);
        }

        public void Initialise(ICodeIssue codeIssue, IReadOnlyList<ICodeActionSuggestion> suggestions, Action<CodeIssueFixSelectedEventArgs> codeFixSelected, string helpUrl)
        {
            try
            {
                Orientation = Orientation.Vertical;

                CodeIssue = codeIssue;
                Suggestions = suggestions;
                CodeFixSelected = codeFixSelected;
                HelpUrl = helpUrl;

                var textColor = TextColorHelper.GetThemeTextBrush();

                summaryLabel = new Label()
                {
                    Content = codeIssue?.Message ?? string.Empty,
                    Foreground = textColor
                };

                Children.Add(summaryLabel);

                if (suggestions != null && suggestions.Any())
                {
                    Children.Add(new Label()
                    {
                        Content = "Available Fixes:",
                        Foreground = textColor,
                    });

                    foreach (var suggestion in suggestions)
                    {
                        var description = suggestion.Description ?? string.Empty;
                        if (!description.EndsWith("."))
                        {
                            description += ".";
                        }

                        var clickable = new ClickableLabel(description, async() =>
                        { 
                            CodeFixSelected?.Invoke(new CodeIssueFixSelectedEventArgs(suggestion));
                        });

                        Children.Add(clickable);
                    }
                }

                Children.Add(new Separator());

                Children.Add(new BrandedFooter(this.HelpUrl));
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
