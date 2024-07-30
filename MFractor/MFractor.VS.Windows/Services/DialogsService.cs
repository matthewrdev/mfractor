using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDialogsService))]
    class DialogsService : IDialogsService
    {
        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public DialogsService(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void ShowError(string message)
        {
            Xwt.MessageDialog.ShowError(message);
        }

        IReadOnlyList<Xwt.Command> AsCommands(IEnumerable<string> choices)
        {
            if (choices == null || !choices.Any())
            {
                return new List<Xwt.Command>();
            }

            return choices.Where(c => !string.IsNullOrEmpty(c)).Distinct().Select(c => new Xwt.Command(c, c)).ToList();
        }

        public async Task<string> AskQuestionAsync(string question, params string[] choices)
        {
            var tcs = new TaskCompletionSource<string>();
            Dispatcher.InvokeOnMainThread(() =>
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
            Dispatcher.InvokeOnMainThread(() =>
            {
                var commands = AsCommands(choices);
                var choice = Xwt.MessageDialog.AskQuestion(question.Message, question.Title, commands.ToArray());
                var result = commands?.FirstOrDefault(b => b == choice)?.Label ?? string.Empty;
                tcs.SetResult(result);
            });

            return await tcs.Task;
        }

        public string AskQuestion(string question, params string[] choices)
        {
            var commands = AsCommands(choices);

            var choice = Xwt.MessageDialog.AskQuestion(question, commands.ToArray());

            return commands?.FirstOrDefault(b => b == choice)?.Label ?? string.Empty;
        }

        public void ShowMessage(string message, string secondaryText)
        {
            Xwt.MessageDialog.ShowMessage(message, secondaryText);
        }

        public bool Confirm(string message, string confirmText)
        {
            return Xwt.MessageDialog.Confirm(message, new Xwt.Command(confirmText));
        }

        public void StatusBarMessage(string message)
        {
            StatusBarMessage(message, string.Empty);
        }

        public void StatusBarMessage(string message, string imageId)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                var statusBar = (IVsStatusbar)ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar));
                statusBar.SetText(message);
            });
        }

        public IProgressMonitor StartStatusBarTask(string message)
        {
            StatusBarMessage(message);

            return new StubProgressMonitor();
        }

        public string AskQuestion(Question question, params string[] choices)
        {
            var commands = AsCommands(choices);

            var choice = Xwt.MessageDialog.AskQuestion(question.Message, question.Title, commands.ToArray());

            return commands?.FirstOrDefault(b => b == choice)?.Label ?? string.Empty;
        }

        public void ToolbarMessage(string message, params ToolBarAction[] actions)
        {
            // Not implemented.
        }

        public void ShowMessage(string message, string secondaryText, object window)
        {
            ShowMessage(message, secondaryText);
        }
    }
}
