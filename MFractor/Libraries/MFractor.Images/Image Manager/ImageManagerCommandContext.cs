using System;
using MFractor.Images.ImageManager;
using MFractor.Images;
using MFractor;
using MFractor.Workspace;

namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// An implementation of the <see cref="IImageManagerCommandContext"/>.
    /// </summary>
    public class ImageManagerCommandContext : IImageManagerCommandContext
    {
        public ImageManagerCommandContext(IImageAsset imageAsset,
                                          IProjectFile projectFile,
                                          IImageManagerOptions imageManagerOptions,
                                          IImageManagerController imageManager)
        {
            ImageAsset = imageAsset;
            ProjectFile = projectFile;
            ImageManagerOptions = imageManagerOptions ?? throw new ArgumentNullException(nameof(imageManagerOptions));
            ImageManagerController = imageManager ?? throw new ArgumentNullException(nameof(imageManager));
        }

        /// <summary>
        /// The targetted image asset.
        /// </summary>
        public IImageAsset ImageAsset { get; }

        /// <summary>
        /// The targetted project file.
        /// </summary>
        public IProjectFile ProjectFile { get; }

        /// <summary>
        /// The options for the current image manager context.
        /// </summary>
        public IImageManagerOptions ImageManagerOptions { get; }

        /// <summary>
        /// The image manager controller for the current context.
        /// </summary>
        public IImageManagerController ImageManagerController { get; }
    }
}