using System;
using System.IO;
using MFractor.Utilities;
using MFractor.Workspace;

namespace MFractor.Images.Utilities
{
    /// <summary>
    /// A collection of extension mehtods for working with 
    /// </summary>
    public static class ImageAssetProjectFileExtensions
    {
        /// <summary>
        /// Is this <paramref name="file"/> an iOS image asset?
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsIOSImageAsset(this IProjectFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var project = file.CompilationProject;

            if (project == null
               || !project.IsAppleUnifiedProject())
            {
                return false;
            }

            var fileInfo = new FileInfo(file.FilePath);

            if (!fileInfo.Directory.Name.Equals("resources", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return ImageHelper.IsImageFile(file.FilePath);
        }

        public static bool IsAndroidImageAsset(this IProjectFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var project = file.CompilationProject;

            if (project == null
                || !project.IsAndroidProject())
            {
                return false;
            }

            if (!ImageHelper.IsImageFile(file.FilePath))
            {
                return false;
            }

            var fileInfo = new FileInfo(file.FilePath);

            return fileInfo.Directory.Name.Equals("resources", StringComparison.OrdinalIgnoreCase)
                         || fileInfo.Directory.Name.StartsWith("mipmap", StringComparison.OrdinalIgnoreCase)
                         || fileInfo.Directory.Name.StartsWith("drawable", StringComparison.OrdinalIgnoreCase);
        }
    }
}
