using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

using MFractor.Progress;
using MFractor.VS.Mac.Progress;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;
using Xwt;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDialogsService))]
    class DialogsService : IDialogsService
    {
        public void ShowError(string message)
        {
            MonoDevelop.Ide.MessageService.ShowError(message);
        }

        IReadOnlyList<MonoDevelop.Ide.AlertButton> AsButtons(IEnumerable<string> choices)
        {
            if (choices == null || !choices.Any())
            {
                return new List<MonoDevelop.Ide.AlertButton>();
            }

            return choices.Where(c => !string.IsNullOrEmpty(c)).Distinct().Select(c => new AlertButton(c)).ToList();
        }

        public string AskQuestion(string question, params string[] choices)
        {
            var buttons = AsButtons(choices);

            var choice = MonoDevelop.Ide.MessageService.AskQuestion(question, buttons.ToArray());

            return buttons?.FirstOrDefault(b => b == choice)?.Label ?? string.Empty;
        }

        public void ShowMessage(string message, string secondaryText, object window)
        {
            if (window is Xwt.WindowFrame xwtWindow)
            {
                MonoDevelop.Ide.MessageService.ShowMessage(xwtWindow, message, secondaryText);
            }
            else
            {
                MonoDevelop.Ide.MessageService.ShowMessage(message, secondaryText);
            }
        }

        public void ShowMessage(string message, string secondaryText)
        {
            if (!string.IsNullOrEmpty(secondaryText) && secondaryText.Equals("ok", System.StringComparison.InvariantCultureIgnoreCase))
            {
                secondaryText = string.Empty;
            }

            MonoDevelop.Ide.MessageService.ShowMessage(message, secondaryText);
        }

        public bool Confirm(string message, string confirmText)
        {
            return MonoDevelop.Ide.MessageService.Confirm(message, new AlertButton(confirmText));
        }

        public const string MFractorLogo16Px = "mfractor-logo-16";

        public void StatusBarMessage(string message)
        {
            StatusBarMessage(message, MFractorLogo16Px);
        }

        public void StatusBarMessage(string message, string imageId)
        {
            Xwt.Application.Invoke(() =>
            {
                MonoDevelop.Ide.IdeApp.Workbench.StatusBar.ShowMessage(imageId, message);
            });
        }

        public string AskQuestion(Question question, params string[] choices)
        {
            var buttons = AsButtons(choices);

            var choice = MonoDevelop.Ide.MessageService.AskQuestion(question.Title, question.Message, buttons.ToArray());

            return buttons?.FirstOrDefault(b => b == choice)?.Label ?? string.Empty;
        }

        IReadOnlyList<Xwt.Command> AsCommands(IEnumerable<string> choices)
        {
            if (choices == null || !choices.Any())
            {
                return new List<Xwt.Command>();
            }

            return choices.Where(c => !string.IsNullOrEmpty(c)).Distinct().Select(c => new Command(c, c)).ToList();
        }

        public async Task<string> AskQuestionAsync(string question, params string[] choices)
        {
            var tcs = new TaskCompletionSource<string>();
            Xwt.Application.Invoke(() =>
            {
                var commands = AsCommands(choices);
                var choice = Xwt.MessageDialog.AskQuestion(question, commands.ToArray());
                var result = commands?.FirstOrDefault(b => b == choice)?.Label ?? string.Empty;
                tcs.SetResult(result);
            });

            return await tcs.Task;
        }

        public async Task<string> AskQuestionAsync(Question question, params string[] choices)
        {
            var tcs = new TaskCompletionSource<string>();
            Xwt.Application.Invoke(() =>
            {
                var commands = AsCommands(choices);
                var choice = Xwt.MessageDialog.AskQuestion(question.Message, question.Title, commands.ToArray());
                var result = commands?.FirstOrDefault(b => b == choice)?.Label ?? string.Empty;
                tcs.SetResult(result);
            });

            return await tcs.Task;
        }

        public IProgressMonitor StartStatusBarTask(string message)
        {
            var progressMonitor = IdeApp.Workbench.ProgressMonitors.GetStatusProgressMonitor(message, Stock.StatusSolutionOperation, false, true, false);

            return new IdeProgressMonitor(string.Empty, progressMonitor);
        }

        public void ToolbarMessage(string message, params ToolBarAction[] actions)
        {
            Xwt.Application.Invoke(() =>
            {
                var items = Enumerable.Empty<InfoBarItem>().ToArray();

                if (actions != null && actions.Any())
                {
                    items = actions?.Select(a => new InfoBarItem(a.Message, InfoBarItemKind.Button, a.Action, true))?.ToArray();
                }

                MonoDevelop.Ide.IdeApp.Workbench.ShowInfoBar(true, new InfoBarOptions(message)
                {
                    Items = items
                });
            });
        }
    }
}
