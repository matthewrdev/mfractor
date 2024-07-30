using System.Collections.Generic;
using MFractor.IOC;
using MFractor.Workspace;

namespace MFractor.Images
{
    /// <summary>
    /// A repository of available <see cref="IImageAssetUsageFinder"/> implementations.
    /// </summary>
    public interface IImageAssetUsageFinderRepository : IPartRepository<IImageAssetUsageFinder>
    {
        /// <summary>
        /// The available <see cref="IImageAssetUsageFinder"/>'s.
        /// </summary>
        IReadOnlyList<IImageAssetUsageFinder> ImageAssetUsageFinders { get; }

        IEnumerable<IImageAssetUsageFinder> GetImageAssetUsageFindersForExtension(string fileExtension);

        IEnumerable<IImageAssetUsageFinder> GetImageAssetUsageFinders(IProjectFile projectFile);
    }
}
