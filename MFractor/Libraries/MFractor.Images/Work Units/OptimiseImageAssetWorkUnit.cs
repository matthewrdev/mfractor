using System.Collections.Generic;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Images.WorkUnits
{
    public delegate void OnImageOptimisationFinishedDelegate();

    /// <summary>
    /// Launches the image optimisation tool.
    /// </summary>
    public class OptimiseImageAssetWorkUnit : WorkUnit
    {
        public OptimiseImageAssetWorkUnit(IReadOnlyList<IImageAsset> assets)
        {
            ImageAssetOptimisationKind = ImageAssetOptimisationKind.Bulk;
            Assets = assets;
        }

        public OptimiseImageAssetWorkUnit(IImageAsset imageAsset)
        {
            ImageAsset = imageAsset;
            ImageAssetOptimisationKind = ImageAssetOptimisationKind.ImageAsset;
        }

        public OptimiseImageAssetWorkUnit(IProjectFile projectFile)
        {
            ProjectFile = projectFile;
            ImageAssetOptimisationKind = ImageAssetOptimisationKind.ProjectFile;
        }

        public IReadOnlyList<IImageAsset> Assets { get; }

        public IImageAsset ImageAsset { get; }

        public IProjectFile ProjectFile { get; }

        public ImageAssetOptimisationKind ImageAssetOptimisationKind { get; }

        public OnImageOptimisationFinishedDelegate OnImageOptimisationFinishedDelegate { get; set; }

        /// <summary>
        /// Before running the image optimisation, should MFractor confirm with the user that they wish to optimise the asset?
        /// </summary>
        public bool RequiresConfirmation { get; set; } = true;
    }
}
