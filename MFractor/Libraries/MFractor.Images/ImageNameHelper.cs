using System;
using System.IO;
using MFractor.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    /// <summary>
    /// A helper class for getting the image asset name from a <see cref="IProjectFile"/>.
    /// </summary>
    public static class ImageNameHelper
    {
        /// <summary>
        /// Gets the image asset name for the given <paramref name="projectFile"/>.
        /// </summary>
        /// <returns>The image asset name.</returns>
        /// <param name="projectFile">Project file.</param>
        public static string GetImageAssetName(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return string.Empty;
            }

            if (!ImageHelper.IsImageFile(projectFile.FilePath))
            {
                return string.Empty;
            }

            if (!projectFile.CompilationProject.IsMobileProject())
            {
                return string.Empty;
            }

            if (projectFile.CompilationProject.IsAppleUnifiedProject())
            {
                if (ImageAssetHelper.IsAppleUnifiedImageSet(projectFile.FileInfo))
                {
                    return GetImageCatalogName(projectFile, projectFile.Extension);
                }
                else if (ImageAssetHelper.IsAppleUnifiedImageResource(projectFile))
                {
                    return GetBundleResourceImageName(projectFile);
                }
                else if (ImageAssetHelper.IsAppleUnifiedAppIconSet(projectFile))
                {
                    return GetImageAppIconSetName(projectFile, projectFile.Extension);
                }
            }
            else if (projectFile.CompilationProject.IsAndroidProject())
            {
                return projectFile.Name;
            }

            return string.Empty;
        }

        public static string GetImageAssetName(Project project, string imageFilePath)
        {
            if (project == null || string.IsNullOrEmpty(imageFilePath))
            {
                return string.Empty;
            }

            if (!ImageHelper.IsImageFile(imageFilePath))
            {
                return string.Empty;
            }

            if (!project.IsMobileProject())
            {
                return Path.GetFileNameWithoutExtension(imageFilePath);
            }

            var fileInfo = new FileInfo(imageFilePath);

            if (project.IsAppleUnifiedProject())
            {
                if (ImageAssetHelper.IsAppleUnifiedImageSet(fileInfo))
                {
                    return GetImageCatalogName(fileInfo, fileInfo.Extension);
                }
                else if (ImageAssetHelper.IsAppleUnifiedImageResource(fileInfo))
                {
                    return GetBundleResourceImageName(fileInfo);
                }
                else if (ImageAssetHelper.IsAppleUnifiedAppIconSet(fileInfo))
                {
                    return GetImageAppIconSetName(fileInfo, fileInfo.Extension);
                }
            }
            else if (project.IsAndroidProject())
            {
                return fileInfo.Name;
            }

            return Path.GetFileNameWithoutExtension(imageFilePath);
        }

        /// <summary>
        /// Assuming that the <paramref name="projectFile"/> is an Apple Unified bundle resource, get's the image asset name.
        /// </summary>
        /// <returns>The bundle resource image name.</returns>
        /// <param name="projectFile">Project file.</param>
        public static string GetBundleResourceImageName(IProjectFile projectFile)
        {
            return GetBundleResourceImageName(projectFile.FilePath);
        }

        /// <summary>
        /// Assuming that the <paramref name="fileInfo"/> is an Apple Unified bundle resource, get's the image asset name.
        /// </summary>
        /// <returns>The bundle resource image name.</returns>
        /// <param name="fileInfo">Project file.</param>
        public static string GetBundleResourceImageName(FileInfo fileInfo)
        {
            return GetBundleResourceImageName(fileInfo.FullName);
        }

        /// <summary>
        /// Assuming that the <paramref name="filePath"/> is an Apple Unified bundle resource, get's the image asset name.
        /// </summary>
        /// <returns>The bundle resource image name.</returns>
        /// <param name="filePath">The file path to the image asset.</param>
        public static string GetBundleResourceImageName(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);

            if (name.EndsWith("@1x", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("@2x", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("@3x", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 3);
            }

            return name;
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetUwpImageName(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);

            if (name.Contains(".scale-"))
            {
                name = name.Substring(0, name.IndexOf(".scale-") );
            }
            else if (name.Contains(".targetsize-"))
            {
                name = name.Substring(0, name.IndexOf(".targetsize-"));
            }

            return name;
        }

        /// <summary>
        /// Assuming that the <paramref name="projectFile"/> is an Apple Unified Image Set, get's the image asset name.
        /// </summary>
        /// <returns>The image catalog name.</returns>
        /// <param name="projectFile">Project file.</param>
        public static string GetImageCatalogName(IProjectFile projectFile, string extension)
        {
            return GetImageCatalogName(projectFile.FileInfo, extension);
        }

        /// <summary>
        /// Assuming that the <paramref name="DirectoryInfo"/> is an Apple Unified Image Set, get's the image asset name.
        /// </summary>
        /// <returns>The image catalog name.</returns>
        /// <param name="projectFile">Project file.</param>
        public static string GetImageCatalogName(DirectoryInfo directory, string extension)
        {
            var folder = directory.Name;

            var imageName = folder.Substring(0, folder.Length - ".imageset".Length) + extension;

            return imageName;
        }

        /// <summary>
        /// Assuming that the <paramref name="fileInfo"/> is an Apple Unified Image Set, get's the image asset name.
        /// </summary>
        /// <returns>The image catalog name.</returns>
        /// <param name="fileInfo">File info.</param>
        public static string GetImageCatalogName(FileInfo fileInfo, string extension)
        {
            var folder = fileInfo.Directory.Name;
            var imageName = folder.Substring(0, folder.Length - ".imageset".Length) + extension;

            return imageName;
        }

        /// <summary>
        /// Assuming that the <paramref name="projectFile"/> is an Apple Unified Image Set, get's the image asset name.
        /// </summary>
        /// <returns>The image catalog name.</returns>
        /// <param name="projectFile">Project file.</param>
        public static string GetImageAppIconSetName(IProjectFile projectFile, string extension)
        {
            var folder = projectFile.FileInfo.Directory.Name;
            var imageName = folder.Substring(0, folder.Length - ".appiconset".Length) + extension;

            return imageName;
        }

        /// <summary>
        /// Assuming that the <paramref name="directoryInfo"/> is an Apple Unified Image Set, get's the image asset name.
        /// </summary>
        /// <returns>The image catalog name.</returns>
        /// <param name="projectFile">Project file.</param>
        public static string GetImageAppIconSetName(DirectoryInfo directoryInfo, string extension)
        {
            var folder = directoryInfo.Name;
            var imageName = folder.Substring(0, folder.Length - ".appiconset".Length) + extension;

            return imageName;
        }

        /// <summary>
        /// Assuming that the <paramref name="fileInfo"/> is an Apple Unified Image Set, get's the image asset name.
        /// </summary>
        /// <returns>The image catalog name.</returns>
        /// <param name="fileInfo">The image file.</param>
        public static string GetImageAppIconSetName(FileInfo fileInfo, string extension)
        {
            var folder = fileInfo.Directory.Name;
            var imageName = folder.Substring(0, folder.Length - ".appiconset".Length) + extension;

            return imageName;
        }
    }
}
