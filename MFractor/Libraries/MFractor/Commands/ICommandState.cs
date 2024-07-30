using System.Collections.Generic;

namespace MFractor.Commands
{
    public interface ICommandState
    {
        bool IsVisible { get; set; }

        bool CanExecute { get; set; }

        bool BlockSubsequentCommands { get; set; }

        bool IsSeparator { get; set; }

        string Label { get; set; }

        string Description { get; set; }

        IEnumerable<ICommand> NestedCommands { get; set; }
    }
}