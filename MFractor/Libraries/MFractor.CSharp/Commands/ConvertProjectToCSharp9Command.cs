using System;
using System.ComponentModel.Composition;
using MFractor.Commands;
using MFractor.CSharp.Services;
using MFractor.Ide.Commands;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ConvertProjectToCSharp9Command))]
    public class ConvertProjectToCSharp9Command : ICommand
    {
        readonly Lazy<IConvertProjectToCSharp9Service> convertService;
        IConvertProjectToCSharp9Service ConvertService => convertService.Value;

        [ImportingConstructor]
        public ConvertProjectToCSharp9Command(Lazy<IConvertProjectToCSharp9Service> convertService) => this.convertService = convertService;

        public void Execute(ICommandContext commandContext)
        {
            var project = GetSelectedProject(commandContext);
            if (project is null)
            {
                return;
            }

            ConvertService.Convert(project);
        }

        Project GetSelectedProject(ICommandContext commandContext)
        {
            return (commandContext as ISolutionPadCommandContext)?.SelectedItem as Project;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var project = GetSelectedProject(commandContext);
            if (project is null)
            {
                return default;
            }
            if (!ConvertService.CanConvertProject(project))
            {
                return default;
            }

            return new CommandState { CanExecute = true };
        }
    }
}
