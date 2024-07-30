using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageAssetUsageService))]
    class ImageAssetUsageService : IImageAssetUsageService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IImageAssetUsageFinderRepository> imageAssetUsageFinderRepository;
        public IImageAssetUsageFinderRepository ImageAssetUsageFinderRepository => imageAssetUsageFinderRepository.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        [ImportingConstructor]
        public ImageAssetUsageService(Lazy<IProjectService> projectService,
                                      Lazy<ITextProviderService> textProviderService,
                                      Lazy<IImageAssetUsageFinderRepository> imageAssetUsageFinderRepository)
        {
            this.projectService = projectService;
            this.textProviderService = textProviderService;
            this.imageAssetUsageFinderRepository = imageAssetUsageFinderRepository;
        }

        public async Task<IEnumerable<IImageAssetUsage>> FindUsages(ProjectIdentifier projectIdentifier, IImageAsset imageAsset, IProgressMonitor progressMonitor)
        {
            var files = ProjectService.GetProjectFiles(projectIdentifier).ToList();

            var usages = new List<IImageAssetUsage>();
            var index = 0;
            foreach (var file in files)
            {
                index++;
                progressMonitor.SetProgress("Searching " + projectIdentifier.Name, (double)index / (double)files.Count);
                var finders = ImageAssetUsageFinderRepository.GetImageAssetUsageFindersForExtension(file.Extension);

                var textProvider = TextProviderService.GetTextProvider(file.FilePath);

                foreach (var finder in finders)
                {
                    try
                    {
                        var result = await finder.FindUsagesAsync(imageAsset, file, textProvider, progressMonitor);

                        if (result != null && result.Any())
                        {
                            usages.AddRange(result);
                        }

                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }
            }

            return usages;
        }

        public async Task<IEnumerable<IImageAssetUsage>> FindUsages(Solution solution, IImageAsset imageAsset, IProgressMonitor progressMonitor)
        {
            var projects = solution.Projects.Select(p => p.GetIdentifier()).ToList();

            return await FindUsages(projects, imageAsset, progressMonitor);
        }

        public async Task<IEnumerable<IImageAssetUsage>> FindUsages(IEnumerable<ProjectIdentifier> projects, IImageAsset imageAsset, IProgressMonitor progressMonitor)
        {
            var usages = new List<IImageAssetUsage>();
            foreach (var pid in projects)
            {
                try
                {
                    var results = await FindUsages(pid, imageAsset, progressMonitor);

                    if (results != null && results.Any())
                    {
                        usages.AddRange(results);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            return usages;
        }

        public async Task<IEnumerable<IImageAssetUsage>> FindUsages(IImageAsset imageAsset, IProgressMonitor progressMonitor)
        {
            var projects = imageAsset.Projects.Select(p => p.GetIdentifier()).ToList();

            return await FindUsages(projects, imageAsset, progressMonitor);
        }
    }
}
