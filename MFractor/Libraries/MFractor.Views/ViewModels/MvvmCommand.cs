using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
//using Microsoft.VisualStudio.Shell;       // TODO: fix this to be cross platform

namespace MFractor.Views.ViewModels
{
    class MvvmCommand : ICommand
    {
        readonly Action executeAction;

        public event EventHandler CanExecuteChanged;
        // TODO: Fix this to be multiplatform.
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        public MvvmCommand(Action executeAction)
        {
            this.executeAction = executeAction;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            executeAction?.Invoke();
        }
    }
}
