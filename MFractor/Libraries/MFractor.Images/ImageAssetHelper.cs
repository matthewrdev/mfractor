using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.Utilities;
using MFractor.Workspace;

namespace MFractor.Images
{
    /// <summary>
    /// The image asset helper is used to find image assets in Android, iOS and Mac projects.
    /// </summary>
    public static class ImageAssetHelper
    {
        public const string ImageTypeAndroidDrawable = "Drawable";

        public const string ImageTypeAndroidMipMap = "MipMap";

        public const string ImageTypeIOSAssetCatalog = "Asset Catalog";

        public const string ImageTypeIOSBundleResource = "Bundle Resource";

        public const string ImageTypeSharedImage = "Shared Image";

        public const string BundleResourceBuildAction = "BundleResource";

        public const string AndroidResourceBuildAction = "AndroidResource";

        public const string MauiImageBuildAction = "MauiImage";

        public const string MauiAppIconBuildAction = "MauiIcon";

        public const string ImageAssetBuildAction = "ImageAsset";

        public const string EmbeddedResourceBuildAction = "EmbeddedResource";

        public const string ContentBuildAction = "Content";

        public static ImageResourceType ToImageResourceType(string kind)
        {
            switch(kind)
            {
                case ImageTypeAndroidDrawable:
                    return ImageResourceType.Drawable;
                case ImageTypeAndroidMipMap:
                    return ImageResourceType.MipMap;
                case ImageTypeIOSAssetCatalog:
                    return ImageResourceType.AssetCatalog;
                case ImageTypeIOSBundleResource:
                    return ImageResourceType.BundleResource;
                case ImageTypeSharedImage:
                    return ImageResourceType.SharedImage;
                case EmbeddedResourceBuildAction:
                    return ImageResourceType.EmbeddedResource;
                case MauiImageBuildAction:
                    return ImageResourceType.MauiImage;
                case MauiAppIconBuildAction:
                    return ImageResourceType.MauiAppIcon;
                case ContentBuildAction:
                    return ImageResourceType.Content;

            }

            return ImageResourceType.BundleResource;
        }

        /// <summary>
        /// Is the provided <paramref name="projectFile"/> an image asset?
        /// </summary>
        /// <returns><c>true</c>, if image asset was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectFile">Project file.</param>
        public static bool IsImageAsset(IProjectFile projectFile)
        {
            if (projectFile == null || projectFile.CompilationProject == null)
            {
                return false;
            }

            if (!projectFile.CompilationProject.IsMobileProject() && !projectFile.CompilationProject.IsMauiProject())
            {
                return false;
            }

            if (IsMauiImage(projectFile) || IsMauiAppIcon(projectFile))
            {
                return true;
            }
            else if (projectFile.CompilationProject.IsAndroidProject())
            {
                return IsAndroidImageResource(projectFile);
            }
            else if (projectFile.CompilationProject.IsAppleUnifiedProject())
            {
                if (IsAppleUnifiedImageResource(projectFile))
                {
                    return true;
                }
                else if (IsAppleUnifiedImageSet(projectFile.FileInfo))
                {
                    return true;
                }

                return IsAppleUnifiedAppIconSet(projectFile);
            }
            else if (projectFile.CompilationProject.IsUWPProject())
            {
                return IsUWPImageResource(projectFile);
            }

            return false;
        }

        public static bool IsUWPImageResource(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != ContentBuildAction)
            {
                return false;
            }

            return ImageHelper.IsImageFile(projectFile.FilePath);
        }

        public static bool IsEmbeddedResourceImageAsset(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != EmbeddedResourceBuildAction)
            {
                return false;
            }

            return ImageHelper.IsImageFile(projectFile.FilePath);
        }

        public static bool IsMauiImage(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != MauiImageBuildAction)
            {
                return false;
            }

            return ImageHelper.IsImageFile(projectFile.FilePath);
        }

        public static bool IsMauiAppIcon(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != MauiAppIconBuildAction)
            {
                return false;
            }

            return ImageHelper.IsImageFile(projectFile.FilePath);
        }

        /// <summary>
        /// Is the provided <paramref name="projectFile"/> an Android image asset?
        /// </summary>
        /// <returns><c>true</c>, if android image asset was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectFile">Project file.</param>
        public static bool IsAndroidImageResource(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != AndroidResourceBuildAction)
            {
                return false;
            }

            return IsAndroidImageResource(projectFile.FileInfo);
        }

        /// <summary>
        /// Is the provided <paramref name="fileInfo"/> an Android drawable or mipmap?
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static bool IsAndroidImageResource(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }

            var folder = fileInfo.Directory.Name;

            if (!(folder.StartsWith("drawable", StringComparison.OrdinalIgnoreCase)
                || folder.StartsWith("mipmap", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (fileInfo.Extension == ".xml")
            {
                return true; // Android Drawable XML.
            }

            return ImageHelper.IsImageFile(fileInfo.FullName);
        }

        public static bool IsAndroidDrawableXmlImageResource(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != AndroidResourceBuildAction)
            {
                return false;
            }

            return IsAndroidDrawableXmlImageResource(projectFile.FileInfo);

        }

        public static bool IsAndroidDrawableXmlImageResource(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }

            var folder = fileInfo.Directory.Name;

            if (!(folder.StartsWith("drawable", StringComparison.OrdinalIgnoreCase)
                || folder.StartsWith("mipmap", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return fileInfo.Extension == ".xml";
        }

        /// <summary>
        /// Is the provided <paramref name="projectFile"/> an iOS or Mac image resource?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image asset was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectFile">Project file.</param>
        public static bool IsAppleUnifiedImageResource(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != BundleResourceBuildAction)
            {
                return false;
            }

            return IsAppleUnifiedImageResource(projectFile.FileInfo);
        }

        /// <summary>
        /// Is the provided <paramref name="fileInfo"/> an iOS or Mac image resource?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image asset was ised, <c>false</c> otherwise.</returns>
        /// <param name="fileInfo">Project file.</param>
        public static bool IsAppleUnifiedImageResource(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }

            return ImageHelper.IsImageFile(fileInfo.FullName);
        }

        /// <summary>
        /// Is the given <paramref name="directoryInfo"/> an Apple Unified asset catalog?
        /// </summary>
        public static bool IsAppleUnifiedAssetCatalog(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                return false;
            }

            return directoryInfo.Name.EndsWith(".xcassets", System.StringComparison.OrdinalIgnoreCase);
        }

        public static IReadOnlyList<FileInfo> GetAssetCatalogFiles(DirectoryInfo directoryInfo)
        {
            if (!IsAppleUnifiedAssetCatalog(directoryInfo))
            {
                return Array.Empty<FileInfo>();
            }

            var files = new List<FileInfo>();

            var innerDirectories = directoryInfo.GetDirectories();

            foreach (var childDirectory in innerDirectories)
            {
                var isCatalog = childDirectory.Name.EndsWith(".imageset", System.StringComparison.OrdinalIgnoreCase)
                                || childDirectory.Name.EndsWith(".appiconset", System.StringComparison.OrdinalIgnoreCase);

                if (!isCatalog)
                {
                    continue;
                }

                files.AddRange(GetImageCatalogFiles(childDirectory));
            }

            return files;
        }


        public static IReadOnlyList<FileInfo> GetImageCatalogFiles(DirectoryInfo directoryInfo)
        {
            if (directoryInfo is null)
            {
                return Array.Empty<FileInfo>();
            }

            var files = directoryInfo.GetFiles();

            return files.Where(f => f.Name == "Contents.json" || ImageHelper.IsBitmapImageFile(f.FullName)).ToList();
        }

        /// <summary>
        /// Is the given <paramref name="projectFile"/> an Apple Unified image set?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image set was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectFile">Project file.</param>
        public static bool IsAppleUnifiedImageSet(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                return false;
            }

            var contentsJson = Path.Combine(directoryInfo.FullName, "Contents.json");

            if (!File.Exists(contentsJson))
            {
                return false;
            }

            return IsAppleUnifiedImageSet(new FileInfo(contentsJson));
        }

        /// <summary>
        /// Is the given <paramref name="projectFile"/> an Apple Unified image set?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image set was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectFile">Project file.</param>
        public static bool IsAppleUnifiedImageSet(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.Extension != ".json")
            {
                return false;
            }

            return IsAppleUnifiedImageSet(projectFile.FileInfo);
        }

        /// <summary>
        /// Is the given <paramref name="fileInfo"/> an Apple Unified image set?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image set was ised, <c>false</c> otherwise.</returns>
        /// <param name="fileInfo">Project file.</param>
        public static bool IsAppleUnifiedImageSet(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }

            if (fileInfo.Name != "Contents.json")
            {
                return false;
            }

            if (!fileInfo.Directory.Parent.Name.EndsWith(".xcassets", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!fileInfo.Directory.Name.EndsWith(".imageset", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Is the given <paramref name="projectFile"/> an Apple Unified image set?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image set was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectFile">Project file.</param>
        public static bool IsAppleUnifiedAppIconSet(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                return false;
            }

            if (projectFile.BuildAction != ImageAssetBuildAction)
            {
                return false;
            }

            return IsAppleUnifiedAppIconSet(projectFile.FileInfo);
        }

        /// <summary>
        /// Is the given <paramref name="projectFile"/> an Apple Unified image set?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image set was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectFile">Project file.</param>
        public static bool IsAppleUnifiedAppIconSet(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                return false;
            }

            var contentsJson = Path.Combine(directoryInfo.FullName, "Contents.json");

            if (!File.Exists(contentsJson))
            {
                return false;
            }

            return IsAppleUnifiedImageSet(new FileInfo(contentsJson));
        }

        /// <summary>
        /// Is the given <paramref name="fileInfo"/> an Apple Unified image set?
        /// </summary>
        /// <returns><c>true</c>, if apple unified image set was ised, <c>false</c> otherwise.</returns>
        /// <param name="fileInfo">The file</param>
        public static bool IsAppleUnifiedAppIconSet(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }

            if (fileInfo.Name != "Contents.json")
            {
                return false;
            }

            if (!fileInfo.Directory.Parent.Name.EndsWith(".xcassets", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var isIconSet = fileInfo.Directory.Name.EndsWith(".appiconset", System.StringComparison.OrdinalIgnoreCase);

            if (!isIconSet)
            {
                return false;
            }

            return true;
        }
    }
}
