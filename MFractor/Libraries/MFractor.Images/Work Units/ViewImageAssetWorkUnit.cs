using System;
using MFractor.Images;
using MFractor.Work;

namespace MFractor.Images.WorkUnits
{
    /// <summary>
    /// Opens the image asset manager and selects the <see cref="ImageAsset"/>.
    /// </summary>
    public class ViewImageAssetWorkUnit : WorkUnit
    {
        public ViewImageAssetWorkUnit(IImageAsset imageAsset)
        {
            ImageAsset = imageAsset;
        }

        /// <summary>
        /// The image asset to view.
        /// </summary>
        /// <value>The image asset.</value>
        public IImageAsset ImageAsset { get; set; }

        /// <summary>
        /// Should the image manager be forced to open if it is not already open?
        /// </summary>
        public bool Force { get; set; } = true;
    }
}
