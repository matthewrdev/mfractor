using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Workspace;

namespace MFractor.Images
{
    /// <summary>
    /// An <see cref="IImageAssetUsageFinder"/> can search a project files for usages of a specified image asset.
    /// <para/>
    /// Implementations of <see cref="IImageAssetUsageFinder"/> are automatically detected and added to the <see cref="IImageAssetUsageFinderRepository"/>.
    /// </summary>
    [InheritedExport]
    public interface IImageAssetUsageFinder
    {
        /// <summary>
        /// The file extensions that this <see cref="IImageAssetUsageFinder"/> supports.
        /// <para/>
        /// Each extension should include the '.'. 
        /// <para/>
        /// For example: { ".resx", ".xml", ".axml" }
        /// </summary>
        /// <value>The supported file extensions.</value>
        string[] SupportedFileExtensions { get; }

        /// <summary>
        /// If this image asset usage finder can operate within the provided <paramref name="projectFile"/>.
        /// </summary>
        bool IsAvailable(IProjectFile projectFile);

        /// <summary>
        /// Asynchronously find the usages of the <paramref name="imageAsset"/> in the given <paramref name="projectFile"/>.
        /// </summary>
        Task<IEnumerable<IImageAssetUsage>> FindUsagesAsync(IImageAsset imageAsset, IProjectFile projectFile, ITextProvider textProvider, IProgressMonitor progressMonitor);
    }
}
