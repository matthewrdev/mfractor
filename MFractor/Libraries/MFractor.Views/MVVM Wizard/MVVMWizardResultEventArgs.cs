using System;
using MFractor.Maui.CodeGeneration.Mvvm;
namespace MFractor.Views.MVVMWizard
{
    public class MVVMWizardResultEventArgs : EventArgs
    {
        public ViewViewModelGenerationOptions Options { get; }

        public MVVMWizardResultEventArgs(ViewViewModelGenerationOptions options)
        {
            Options = options;
        }
    }
}
