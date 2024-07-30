using MFractor.Commands;

namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// The base class for an <see cref="IImageManagerCommand"/>.
    /// </summary>
    public abstract class ImageManagerCommand : IImageManagerCommand
    {
        /// <summary>
        /// The analytics event to fire for this command.
        /// </summary>
        public abstract string AnalyticsEvent { get; }

        public void Execute(ICommandContext commandContext)
        {
            if (commandContext is IImageManagerCommandContext imageManagerCommandContext)
            {
                OnExecute(imageManagerCommandContext);
            }
        }

        /// <summary>
        /// Execute this image manager command.
        /// </summary>
        /// <param name="commandContext"></param>
        protected abstract void OnExecute(IImageManagerCommandContext commandContext);

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            if (commandContext is IImageManagerCommandContext imageManagerCommandContext)
            {
                return OnGetExecutionState(imageManagerCommandContext);
            }

            return default;
        }

        /// <summary>
        /// Get the <see cref="ICommandState"/> for this image manager command.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        protected abstract ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext);
    }
}