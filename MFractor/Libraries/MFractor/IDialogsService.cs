using System;
using System.Threading.Tasks;
using MFractor.Progress;

namespace MFractor
{
    public interface IDialogsService
    {
        void ShowError(string message);

        /// <summary>
        /// Display a dialog asking a question to the user with the choices as buttons.
        /// The selected choice is returned.
        /// </summary>
        string AskQuestion(string question, params string[] choices);

        /// <summary>
        /// Display a dialog asking a question to the user with the choices as buttons.
        /// The selected choice is returned.
        /// </summary>
        Task<string> AskQuestionAsync(string question, params string[] choices);

        string AskQuestion(Question question, params string[] choices);

        Task<string> AskQuestionAsync(Question question, params string[] choices);

        void ShowMessage(string message, string confirmText);

        void StatusBarMessage(string message);

        void StatusBarMessage(string message, string imageId);

        void ToolbarMessage(string message, params ToolBarAction[] actions);

        IProgressMonitor StartStatusBarTask(string message);

        bool Confirm(string message, string confirmText);

        void ShowMessage(string message, string secondaryText, object window);
    }
}
