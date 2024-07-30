using MFractor.Images.ImageManager;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.WorkUnits
{
    /// <summary>
    /// Requests that MFractor opens the image manager.
    /// </summary>
    public class OpenImageManagerWorkUnit : WorkUnit
    {
        /// <summary>
        /// The solution to open the image manager for.
        /// <para/>
        /// When null, uses the currently open solution in the active workspace.
        /// </summary>
        public Solution Solution { get; set; }

        /// <summary>
        /// The behaviour to launch the image manager with.
        /// <para/>
        /// Defaults to <see cref="ImageManagerOptions.Default"/>.
        /// </summary>
        public IImageManagerOptions Options { get; set; } = ImageManagerOptions.Default;

        /// <summary>
        /// The image asset to select when the image asset manager opens.
        /// <para/>
        /// When null, the image manager opens with no selection.
        /// </summary>
        public string SelectedImageAsset { get; set; }

        /// <summary>
        /// If the image manager is not already open, should it be forced to display?
        /// </summary>
        public bool Force { get; set; } = true;

        /// <summary>
        /// When the image manager pad opens, should the purchase push be supressed for non-license holders?
        /// </summary>
        public bool ShouldSuppressPurchasePush { get; set; } = false;
    }
}
