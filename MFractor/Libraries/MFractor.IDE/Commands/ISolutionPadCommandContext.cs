using MFractor.Commands;

namespace MFractor.Ide.Commands
{
    /// <summary>
    /// A <see cref="ICommandContext"/> that signifies the command has been triggered from the solution pad / solution explorer.
    /// <para/>
    /// This command context includes the <see cref="SelectedItem"/> property that should be type cast to <see cref="Microsoft.CodeAnalysis.Solution"/>, <see cref="Microsoft.CodeAnalysis.Project"/>,
    /// <see cref="IProjectFolder"/> or <see cref="IProjectFile"/> to discover if a command can consume the context,
    /// </summary>
    public interface ISolutionPadCommandContext : ICommandContext
    {
        /// <summary>
        /// The currently selected item in the solution pad.
        /// <para/>
        /// This item should be typecast to <see cref="Microsoft.CodeAnalysis.Solution"/>, <see cref="Microsoft.CodeAnalysis.Project"/>,
        /// <see cref="IProjectFolder"/> or <see cref="IProjectFile"/> to discover if a command can consume the context,
        /// </summary>
        object SelectedItem { get; }
    }
}
