using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Progress;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    public interface IImageAssetUsageService
    {
        Task<IEnumerable<IImageAssetUsage>> FindUsages(IImageAsset imageAsset, IProgressMonitor progressMonitor);

        Task<IEnumerable<IImageAssetUsage>> FindUsages(Solution solution, IImageAsset imageAsset, IProgressMonitor progressMonitor);

        Task<IEnumerable<IImageAssetUsage>> FindUsages(IEnumerable<ProjectIdentifier> projects, IImageAsset imageAsset, IProgressMonitor progressMonitor);

        Task<IEnumerable<IImageAssetUsage>> FindUsages(ProjectIdentifier projectIdentifier, IImageAsset imageAsset, IProgressMonitor progressMonitor);
    }
}
