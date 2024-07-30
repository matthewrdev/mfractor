using System.Collections.Generic;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    /// <summary>
    /// An image asset that is within an Android, iOS or Mac project.
    /// </summary>
    public interface IImageAsset
    {
        /// <summary>
        /// The name of the image asset, including the file extension.
        /// </summary>
        /// <value>The name of the image.</value>
        string Name { get; }

        /// <summary>
        /// The name of the image asset without any file extension component.
        /// </summary>
        /// <value>The image name without extension.</value>
        string ImageNameWithoutExtension { get; }

        /// <summary>
        /// The search name of the image asset, that is, the <see cref="Name"/> in lower case format with diacritics removed.
        /// </summary>
        /// <value>The name of the search.</value>
        string SearchName { get; }

        /// <summary>
        /// All image assets, grouped by project.
        /// </summary>
        /// <value>The assets.</value>
        IReadOnlyDictionary<Project, List<IProjectFile>> Assets { get; }

        /// <summary>
        /// All images that represent this asset.
        /// </summary>
        /// <value>All assets.</value>
        IReadOnlyList<IProjectFile> AllAssets { get; }

        /// <summary>
        /// A file path for the preview of this image.
        /// </summary>
        /// <value>The preview image file path.</value>
        string PreviewImageFilePath { get; }

        /// <summary>
        /// The file extension for this image asset. May be null if this image asset is an asset set only.
        /// </summary>
        /// <value>The extension.</value>
        string Extension { get; }

        /// <summary>
        /// The projects that this image is available within.
        /// </summary>
        /// <value>The projects.</value>
        IReadOnlyList<Project> Projects { get; }

        /// <summary>
        /// Gets the assets for the <paramref name="project"/>.
        /// </summary>
        /// <returns>The assets for.</returns>
        /// <param name="project">Project.</param>
        IReadOnlyList<IProjectFile> GetAssetsFor(Project project);

        /// <summary>
        /// The kinds of image asset this <see cref="IImageAsset"/> is.
        /// </summary>
        IReadOnlyList<ImageAssetKind> ImageAssetKinds { get; }
    }
}