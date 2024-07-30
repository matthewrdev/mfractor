using System;
using MFractor.Utilities;

namespace MFractor.Workspace.Utilities
{
    /// <summary>
    /// A collection of extension methods
    /// </summary>
    public static class ProjectFolderExtensions
    {
        /// <summary>
        /// Is the given <paramref name="folder"/> an image folder for an Apple Unified app kind.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static bool IsAppleUnifiedImageFolder(this IProjectFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            var project = folder.Project;

            if (project == null
               || !project.IsAppleUnifiedProject())
            {
                return false;
            }

            return folder.Name.Equals("resources", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Is the given <paramref name="folder"/> an Android image type folder?
        /// <para/>
        /// For example, is it a mipmap or drawable folder?
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static bool IsAndroidImageFolder(this IProjectFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            var project = folder.Project;

            if (project == null
                || !project.IsAndroidProject())
            {
                return false;
            }

            return folder.Name.Equals("resources", StringComparison.OrdinalIgnoreCase)
                         || folder.Name.StartsWith("mipmap", StringComparison.OrdinalIgnoreCase)
                         || folder.Name.StartsWith("drawable", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Is the given <paramref name="folder"/> the Android assets folder?
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static bool IsAndroidAssetsFolder(this IProjectFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            var project = folder.Project;

            if (project == null
                || !project.IsAndroidProject())
            {
                return false;
            }

            return folder.Name.Equals("assets", StringComparison.OrdinalIgnoreCase);
        }
    }
}
