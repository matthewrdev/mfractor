using System;
using System.Collections.Generic;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Images.WorkUnits
{
    /// <summary>
    /// Launches the image asset deletion tool.
    /// </summary>
    public class DeleteImageAssetWorkUnit : WorkUnit
    {
        public IImageAsset ImageAsset { get; set; }

        public Action<IReadOnlyList<IProjectFile>> OnImagesDeleted { get; set; }
    }
}
