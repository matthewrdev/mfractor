using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    /// <summary>
    /// An image asset that is within an Android, iOS or Mac project.
    /// </summary>
    class ImageAsset : IImageAsset
    {
        /// <summary>
        /// The name of the image asset, including the file extension.
        /// </summary>
        /// <value>The name of the image.</value>
        public string Name { get; }

        /// <summary>
        /// The name of the image asset without any file extension component.
        /// </summary>
        /// <value>The image name without extension.</value>
        public string ImageNameWithoutExtension { get; }

        /// <summary>
        /// The search name of the image asset, that is, the <see cref="Name"/> with diacritics removed.
        /// </summary>
        /// <value>The name of the search.</value>
        public string SearchName { get; }

        readonly Dictionary<Project, List<IProjectFile>> assets = new Dictionary<Project, List<IProjectFile>>();

        /// <summary>
        /// All image assets, grouped by project.
        /// </summary>
        /// <value>The assets.</value>
        public IReadOnlyDictionary<Project, List<IProjectFile>> Assets => assets;

        /// <summary>
        /// All images that represent this asset.
        /// </summary>
        /// <value>All assets.</value>
        public IReadOnlyList<IProjectFile> AllAssets
        {
            get
            {
                var result = new List<IProjectFile>();

                foreach (var pair in Assets)
                {
                    result.AddRange(pair.Value);
                }

                return result;
            }
        }

        readonly Lazy<string> previewImageFilePath;

        /// <summary>
        /// A file path for the preview of this image.
        /// </summary>
        /// <value>The preview image file path.</value>
        public string PreviewImageFilePath => previewImageFilePath.Value;

        public IReadOnlyList<ImageAssetKind> ImageAssetKinds { get; }

        /// <summary>
        /// The file extension for this image asset. May be null if this image asset is an asset set only.
        /// </summary>
        /// <value>The extension.</value>
        public string Extension { get; }

        /// <summary>
        /// The projects that this image is available within.
        /// </summary>
        /// <value>The projects.</value>
        public IReadOnlyList<Project> Projects => Assets.Keys.ToList();

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Images.ImageAsset"/> class.
        /// </summary>
        /// <param name="imageName">Image name.</param>
        /// <param name="previewImageFilePath">Preview image file path.</param>
        public ImageAsset(string imageName, string extension, IEnumerable<ImageAssetKind> imageAssetKinds = null)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                throw new ArgumentException("message", nameof(imageName));
            }

            Name = Path.GetFileNameWithoutExtension(imageName);
            SearchName = imageName.ToLower().RemoveDiacritics();
            previewImageFilePath = new Lazy<string>(() =>
            {
                return AllAssets.FirstOrDefault(a => ImageHelper.IsImageFile(a.FilePath))?.FilePath;
            });

            ImageAssetKinds = (imageAssetKinds ?? Enumerable.Empty<ImageAssetKind>()).ToList();

            if (!ImageAssetKinds.Any())
            {
                ImageAssetKinds = new List<ImageAssetKind>()
                {
                    ImageAssetKind.Resource
                };
            }

            Extension = extension;
        }

        /// <summary>
        /// Add the specified project and file.
        /// </summary>
        /// <param name="project">Project.</param>
        /// <param name="file">File.</param>
        public void Add(Project project, IProjectFile file)
        {
            if (!assets.ContainsKey(project))
            {
                assets.Add(project, new List<IProjectFile>());
            }

            assets[project].Add(file);
        }

        /// <summary>
        /// Gets the assets for the <paramref name="project"/>.
        /// </summary>
        /// <returns>The assets for.</returns>
        /// <param name="project">Project.</param>
        public IReadOnlyList<IProjectFile> GetAssetsFor(Project project)
        {
            if (project == null)
            {
                return null;
            }

            if (!Assets.ContainsKey(project))
            {
                return null;
            }

            return Assets[project];
        }
    }
}
