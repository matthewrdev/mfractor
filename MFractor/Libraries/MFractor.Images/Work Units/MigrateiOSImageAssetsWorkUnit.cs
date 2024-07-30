using System;
using System.Collections.Generic;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.WorkUnits
{
    /// <summary>
    /// 
    /// </summary>
    public class MigrateiOSImageAssetsWorkUnit : WorkUnit
    {
        /// <summary>
        /// The project containing the image assets to migrate.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// The image assets to migrate from bundle resources to 
        /// </summary>
        public IReadOnlyList<IImageAsset> ImageAssets { get; set; }
    }
}
