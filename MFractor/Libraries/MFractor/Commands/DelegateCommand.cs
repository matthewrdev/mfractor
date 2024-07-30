using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace MFractor.Commands
{
    /// <summary>
    /// A <see cref="ICommand"/> implementation that invokes a delegate.
    /// </summary>
    [PartNotDiscoverable]
    public class DelegateCommand : ICommand
    {
        readonly Func<bool> canExecuteAction;
        readonly Action executeAction;

        /// <summary>
        /// Initialises a new instance of <see cref="DelegateCommand"/> with that displays <paramref name="label"/> and that executes '<paramref name="canExecuteAction"/>' to check if <paramref name="executeAction"/> can execute.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="canExecuteAction"></param>
        /// <param name="executeAction"></param>
        public DelegateCommand(string label, Func<bool> canExecuteAction, Action executeAction)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            Label = label;

            this.canExecuteAction = canExecuteAction ?? throw new ArgumentNullException(nameof(canExecuteAction));
            this.executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
        }
        /// <summary>
        /// Initialises a new instance of <see cref="DelegateCommand"/> with that displays the <paramref name="label"/> and <paramref name="description"/> and that executes '<paramref name="canExecuteAction"/>' to check if <paramref name="executeAction"/> can execute.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="description"></param>
        /// <param name="canExecuteAction"></param>
        /// <param name="executeAction"></param>
        public DelegateCommand(string label, string description, Func<bool> canExecuteAction, Action executeAction)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("message", nameof(description));
            }

            Label = label;
            Description = description;

            this.canExecuteAction = canExecuteAction ?? throw new ArgumentNullException(nameof(canExecuteAction));
            this.executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
        }

        /// <summary>
        /// The label for the <see cref="DelegateCommand"/> to display in the user interface.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// The description for the <see cref="DelegateCommand"/> to display in the user interface.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Execute the command given the provided <paramref name="commandContext"/>.
        /// </summary>
        /// <param name="commandContext"></param>
        public void Execute(ICommandContext commandContext)
        {
            executeAction();
        }

        /// <summary>
        /// The execution state of this command, that is, can this command execute, what is it's label/description and any child commands it should show.
        /// <para/>
        /// It is safe to return null from this method.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState(true, canExecuteAction(), Label, Description);
        }
    }
}
