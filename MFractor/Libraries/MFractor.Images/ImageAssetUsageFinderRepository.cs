using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using MFractor.Workspace;

namespace MFractor.Images
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageAssetUsageFinderRepository))]
    class ImageAssetUsageFinderRepository : PartRepository<IImageAssetUsageFinder>, IImageAssetUsageFinderRepository
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<IImageAssetUsageFinder>>> fileExtensionIndexedImageAssetUsageFinders;
        public IReadOnlyDictionary<string, IReadOnlyList<IImageAssetUsageFinder>> FileExtensionIndexedImageAssetUsageFinders => fileExtensionIndexedImageAssetUsageFinders.Value;

        [ImportingConstructor]
        public ImageAssetUsageFinderRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
            fileExtensionIndexedImageAssetUsageFinders = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<IImageAssetUsageFinder>>>(() =>
            {
                var result = new Dictionary<string, List<IImageAssetUsageFinder>>();

                foreach (var finder in ImageAssetUsageFinders)
                {
                    var extensions = finder.SupportedFileExtensions;

                    foreach (var ex in extensions)
                    {
                        if (string.IsNullOrEmpty(ex))
                        {
                            log?.Warning($"The image usage finder {finder.GetType()} is trying to register a null file extension. This will be ignored.");
                            continue;
                        }

                        if (!result.ContainsKey(ex))
                        {
                            result[ex] = new List<IImageAssetUsageFinder>();
                        }

                        result[ex].Add(finder);
                    }
                }

                return result.ToDictionary(kp => kp.Key, kp => (IReadOnlyList<IImageAssetUsageFinder>)kp.Value);
            });
        }

        public IReadOnlyList<IImageAssetUsageFinder> ImageAssetUsageFinders => Parts;

        public IEnumerable<IImageAssetUsageFinder> GetImageAssetUsageFinders(IProjectFile projectFile)
        {
            return ImageAssetUsageFinders.Where(f => f.IsAvailable(projectFile));
        }

        public IEnumerable<IImageAssetUsageFinder> GetImageAssetUsageFindersForExtension(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
            {
                return Enumerable.Empty<IImageAssetUsageFinder>();
            }

            if (!FileExtensionIndexedImageAssetUsageFinders.ContainsKey(fileExtension))
            {
                return Enumerable.Empty<IImageAssetUsageFinder>();
            }

            return FileExtensionIndexedImageAssetUsageFinders[fileExtension];
        }
    }
}