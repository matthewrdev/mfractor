using System;
using MFractor.Commands;
using MFractor.Workspace;

namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// An <see cref="ICommandContext"/> that provides the contextual information for when an <see cref="IImageManagerCommand"/> is being triggered from within the image manager.
    /// </summary>
    public interface IImageManagerCommandContext : ICommandContext
    {
        /// <summary>
        /// The targetted image asset.
        /// </summary>
        IImageAsset ImageAsset { get; }

        /// <summary>
        /// The targetted project file.
        /// </summary>
        IProjectFile ProjectFile { get; }

        /// <summary>
        /// The options for the current image manager context.
        /// </summary>
        IImageManagerOptions ImageManagerOptions { get; }

        /// <summary>
        /// The image manager controller for the current context.
        /// </summary>
        IImageManagerController ImageManagerController { get; }
    }
}
