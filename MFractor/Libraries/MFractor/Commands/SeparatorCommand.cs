using System;
using System.ComponentModel.Composition;

namespace MFractor.Commands
{
    /// <summary>
    /// A <see cref="ICommand"/> implementation to add a separator element to a <see cref="CompositeCommand"/>.
    /// </summary>
    [PartNotDiscoverable]
    public sealed class SeparatorCommand : ICommand
    {
        /// <summary>
        /// Execute the command given the provided <paramref name="commandContext"/>.
        /// </summary>
        /// <param name="commandContext"></param>
        public void Execute(ICommandContext commandContext)
        {
            // Do nothing.
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
            return CommandState.Separator();
        }
    }
}
