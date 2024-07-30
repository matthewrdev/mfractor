using System.Collections.Generic;
using System.Linq;

namespace MFractor.Commands
{
    public class CommandState : ICommandState
    {
        public CommandState()
        {
        }

        public CommandState(bool visible, bool canExecute, string label, string description, IEnumerable<ICommand> nestedCommands = null)
        {
            IsVisible = visible;
            CanExecute = canExecute;
            Label = label;
            Description = description;
            NestedCommands = nestedCommands ?? Enumerable.Empty<ICommand>();
        }

        public CommandState(string label, string description)
        {
            IsVisible = true;
            CanExecute = true;
            Label = label;
            Description = description;
        }

        public bool IsVisible { get; set; } = true;

        public bool CanExecute { get; set; } = true;

        public bool BlockSubsequentCommands { get; set; } = false;

        public bool IsSeparator { get; set; } = false;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; }

        public IEnumerable<ICommand> NestedCommands { get; set; } = Enumerable.Empty<ICommand>();

        public static CommandState Separator()
        {
            return new CommandState()
            {
                IsSeparator = true,
            };
        }

        public static CommandState BlockOthers
        {
            get;
        } = new CommandState()
        {
            BlockSubsequentCommands = true,
            IsVisible = false,
            CanExecute = false,
        };
    }
}
