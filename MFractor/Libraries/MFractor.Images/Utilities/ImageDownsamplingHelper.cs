using System;
using System.IO;
using MFractor.Utilities;

namespace MFractor.Images.Utilities
{
    /// <summary>
    /// Image downsampling helper.
    /// </summary>
    public static class ImageDownsamplingHelper
    {
        /// <summary>
        /// The name of the Default Asset Catalog considered for projects.
        /// </summary>
        public const string DefaultAssetCatalogName = "Assets.xcassets";

        /// <summary>
        /// Gets the project virtual file path for a new image of <paramref name="density"/>.
        /// </summary>
        /// <returns>The virtual file path.</returns>
        /// <param name="operation">Operation.</param>
        /// <param name="density">Density.</param>
        /// <param name="includeResourcesPrefix">Should the virtual folder path include the 'Resources' folder prepended to the start of it?.</param>
        public static string GetVirtualFilePath(ImportImageOperation operation, ImageDensity density, bool includeResourcesPrefix = true)
        {
            if (operation.TargetProject.IsAppleUnifiedProject())
            {
                return GetIOSVirtualFilePath(operation, density, includeResourcesPrefix);
            }
            else if (operation.TargetProject.IsAndroidProject())
            {
                return GetAndroidVirtualFilePath(operation, density, includeResourcesPrefix);
            }

            return operation.ImageName;
        }

        /// <summary>
        /// Gets the Android project virtual file path for a new image of <paramref name="density"/>.
        /// </summary>
        /// <returns>The android virtual file path.</returns>
        /// <param name="operation">Operation.</param>
        /// <param name="density">Density.</param>
        /// <param name="includeResourcesPrefix">Should the virtual folder path include the 'Resources' folder prepended to the start of it?.</param>
        public static string GetAndroidVirtualFilePath(ImportImageOperation operation, ImageDensity density, bool includeResourcesPrefix = true)
        {
            var sourceImageFile = new FileInfo(operation.AnyAppearanceImageFilePath);
            var newImageName = new FileInfo(operation.ImageName);
            var fileName = Path.GetFileNameWithoutExtension(operation.ImageName);
            if (string.IsNullOrEmpty(newImageName.Extension) == false)
            {
                fileName += newImageName.Extension;
            }
            else
            {
                fileName += sourceImageFile.Extension;
            }

            var densityName = operation.ResourceType.ToString().ToLower() + "-" + density.Name;

            if (!string.IsNullOrEmpty(operation.FolderPath))
            {
                fileName = operation.FolderPath + "_" + fileName;
            }

            if (!includeResourcesPrefix)
            {
                return Path.Combine(densityName, fileName);
            }

            return Path.Combine("Resources", densityName, fileName);
        }

        /// <summary>
        /// Gets the iOS project virtual file path for a new image of <paramref name="density"/>.
        /// </summary>
        /// <returns>The IOS Virtual file path.</returns>
        /// <param name="operation">Operation.</param>
        /// <param name="density">Density.</param>
        /// <param name="includeResourcesPrefix">Should the virtual folder path include the 'Resources' folder prepended to the start of it?.</param>
        public static string GetIOSVirtualFilePath(ImportImageOperation operation, ImageDensity density, bool includeResourcesPrefix = true)
        {
            var fileName = GetIOSImageFileNameWithDensity(operation, density);
            if (!string.IsNullOrEmpty(operation.FolderPath) && operation.ResourceType == ImageResourceType.BundleResource)
            {
                fileName = Path.Combine(operation.FolderPath, fileName);
            }

            if (operation.ResourceType == ImageResourceType.AssetCatalog)
            {
                var imageSetFolder = GetIOSImageSetFolderName(operation.ImageName);
                fileName = Path.Combine(imageSetFolder, fileName);
            }

            if (!includeResourcesPrefix)
            {
                return fileName;
            }

            if (operation.ResourceType == ImageResourceType.AssetCatalog)
            {
                return Path.Combine(DefaultAssetCatalogName, fileName);
            }

            return Path.Combine("Resources", fileName);
        }

        /// <summary>
        /// Gets the iOS filename of an image including it's density suffix.
        /// </summary>
        /// <param name="operation">Operation.</param>
        /// <param name="density">Density.</param>
        /// <returns></returns>
        public static string GetIOSImageFileNameWithDensity(ImportImageOperation operation, ImageDensity density)
        {
            var sourceImageFile = new FileInfo(operation.AnyAppearanceImageFilePath);
            var suffix = density.Name == "@1x" ? "" : density.Name;
            var fileName = Path.GetFileNameWithoutExtension(operation.ImageName);

            fileName += suffix;

            var newImageName = new FileInfo(operation.ImageName);
            if (string.IsNullOrEmpty(newImageName.Extension) == false)
            {
                fileName += newImageName.Extension;
            }
            else
            {
                fileName += sourceImageFile.Extension;
            }

            return fileName;
        }

        /// <summary>
        /// Gets the iOS project virtual file path for a new image inside an Asset Catalog.
        /// </summary>
        /// <param name="imageName">The name of the file being created.</param>
        /// <param name="assetCatalogName">
        /// The name of the asset catalog. Defaults 'Assets.xcasset'. Provided here to allow future
        /// ability to change the asset catalog where the imageset will be created.
        /// </param>
        /// <returns>The iOS Virtual Path for the file.</returns>
        public static string GetIOSAssetCatalogImageSetVirtualPath(string imageName, string assetCatalogName = "Assets.xcassets") =>
            Path.Combine(assetCatalogName, GetIOSImageSetFolderName(imageName));

        /// <summary>
        /// Gets the name of the folder for an ImageSet based on it's filename.
        /// </summary>
        /// <param name="imageName">The name of the image file. Can include extension.</param>
        /// <returns>The folder for the imageset based on the filename.</returns>
        public static string GetIOSImageSetFolderName(string imageName)
        {
            var assetName = Path.GetFileNameWithoutExtension(imageName);
            return $"{assetName}.imageset";
        }

        /// <summary>
        /// Compute the new size of the <paramref name="sourceImageSize"/> given the <paramref name="newDensityScale"/>.
        /// </summary>
        /// <returns>The transformed image size.</returns>
        /// <param name="sourceImageSize">Source image size.</param>
        /// <param name="newDensityScale">New density scale.</param>
        /// <param name="sourceDensityScale">Source density scale.</param>
        public static ImageSize GetTransformedImageSize(ImageSize sourceImageSize,
                                                   double newDensityScale,
                                                   double sourceDensityScale)
        {
            var newWidth = sourceImageSize.Width * (newDensityScale / sourceDensityScale);
            var newHeight = sourceImageSize.Height * (newDensityScale / sourceDensityScale);

            return new ImageSize((int)Math.Floor(newWidth), (int)Math.Floor(newHeight));
        }
    }
}
