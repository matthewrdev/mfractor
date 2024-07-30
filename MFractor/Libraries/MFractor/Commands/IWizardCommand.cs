using System;
using System.ComponentModel.Composition;

namespace MFractor.Commands
{
    /// <summary>
    /// A command that opens a wizard.
    /// <para/>
    /// These commands appear in the top "Wizards" menu.
    /// <para/>
    /// Implementations of these commands should handle the project pad context (provided using <see cref="SolutionPadCommandContext.SelectedItem"/> and a general solution wide context.
    /// <para/>
    /// Implementations of this command are automatically added to the top "Wizards" menu and the "Wizards" menu of the solution pad context menu.
    /// </summary>
    [InheritedExport]
    public interface IWizardCommand : ICommand
    {
    }
}
