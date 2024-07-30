using System;
using System.Windows.Input;

#if VS_WINDOWS
using Microsoft.VisualStudio.Shell;
#endif

using Task = System.Threading.Tasks.Task;

namespace MFractor.Views.ViewModels
{
    class AsyncMvvmCommand : ICommand
    {
        readonly Func<Task> executeAction;

#if VS_WINDOWS
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
#else
        public event EventHandler CanExecuteChanged;
#endif

        public AsyncMvvmCommand(Func<Task> executeAction)
        {
            this.executeAction = executeAction;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (executeAction != null)
            {
#if VS_WINDOWS
                ThreadHelper.JoinableTaskFactory.Run(executeAction);
#else
                Task.Run(async () => await executeAction());                
#endif
            }
        }
    }
}
