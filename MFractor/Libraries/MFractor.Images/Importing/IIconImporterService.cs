using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MFractor.Images.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Importing
{
    /// <summary>
    /// Defines an interface for the Icon Importing features.
    /// </summary>
    public interface IIconImporterService
    {
        /// <summary>
        /// Cleans up existing Application icon from the project.
        /// </summary>
        /// <param name="targetProject">The project to clean existing icon from.</param>
        Task CleanupAppIconAsync(Project targetProject);

        /// <summary>
        /// Import the application icons to the target project.
        /// </summary>
        /// <param name="icons">The icons images to be imported.</param>
        /// <param name="sourceImagePath">The path to the source image that will be used on the import operation.</param>
        /// <param name="targetProject">The project to which import the icons.</param>
        /// <returns>A Task that will return a boolean indicating if the operation has been completed.</returns>
        Task<bool> ImportIconAsync(IEnumerable<IconImage> icons, string sourceImagePath, Project targetProject);

        /// <summary>
        /// Import the application icon from a group of selected icons.
        /// </summary>
        /// <param name="iconSets">The enumerable of icon sets to import images from.</param>
        /// <param name="sourceImagePath">The path for the image that will be used as icon.</param>
        /// <param name="targetProject">The destination project to import the icons.</param>
        /// <returns>A Task that will return a boolean indicating if the operation has been completed.</returns>
        Task<bool> ImportIconAsync(IEnumerable<AppIconSet> iconSets, string sourceImagePath, Project targetProject);
    }
}
