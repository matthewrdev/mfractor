namespace MFractor.Commands
{
    /// <summary>
    /// A default implementation of the <see cref="ICommandContext"/> to provide into <see cref="ICommand"/>'s.
    /// </summary>
    public sealed class DefaultCommandContext : ICommandContext
    {
        /// <summary>
        /// A default implementation of the <see cref="ICommandContext"/> to provide into <see cref="ICommand"/>'s.
        /// </summary>
        public static readonly DefaultCommandContext Instance = new DefaultCommandContext();
    }
}