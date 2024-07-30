using System.ComponentModel.Composition;
using MFractor.Analytics;

namespace MFractor.Commands
{
    /// <summary>
    /// A command that can be executed.
    /// <para/>
    /// Implementations of <see cref="ICommand"/> are automatically detected and added into the <see cref="ICommandRepository"/>.
    /// <para/>
    /// If you are creating a new generic command type like the <see cref="DelegateCommand"/> or <see cref="CompositeCommand"/>, please exclude the automatic command registration from occuring by applying the <see cref="PartNotDiscoverableAttribute"/> onto the command declaration.
    /// <para/>
    /// When a command implements <see cref="IAnalyticsFeature"/>, the adapter layer in the outer IDE will track its <see cref="IAnalyticsFeature.AnalyticsEvent"/> automatically when the command is invoked successfully.
    /// </summary>
    [InheritedExport]
    public interface ICommand
    {
        /// <summary>
        /// The execution state of this command. For example: can this command execute, what is it's label and description and any child commands it should show.
        /// <para/>
        /// It is safe to return null from this method.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        ICommandState GetExecutionState(ICommandContext commandContext);

        /// <summary>
        /// Execute the command given the provided <paramref name="commandContext"/>.
        /// </summary>
        /// <param name="commandContext"></param>
        void Execute(ICommandContext commandContext);
    }
}
