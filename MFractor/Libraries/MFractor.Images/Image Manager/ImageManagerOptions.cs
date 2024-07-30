namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// Provides default implementations of the <see cref="IImageManagerOptions"/> interface.
    /// <para/>
    ///  - <see cref="Browse"/>: The image manager should only allow browsing of image assets.
    /// <para/>
    ///  - <see cref="Choose"/>: The image manager should allow browsing and the ability to choose an image asset.
    /// <para/>
    ///  - <see cref="Delete"/>: The image manager should allow browsing and deletion of image assets.
    /// <para/>
    ///  - <see cref="Import"/>: The image manager should allow browsing and importing of image assets.
    /// <para/>
    ///  - <see cref="Edit"/>: The image manager should allow browsing, deletion, importing and all editing actions for image assets.
    /// <para/>
    ///  - <see cref="Default"/>: The image manager should use it's default behaviour, equivalent to <see cref="Edit"/>.
    /// </summary>
    public class ImageManagerOptions : IImageManagerOptions
    {
        /// <summary>
        /// The image manager should only allow browsing of image assets.
        /// </summary>
        public static readonly ImageManagerOptions Browse = new ImageManagerOptions(false, false, false);

        /// <summary>
        /// The image manager should allow browsing and the ability to choose an image asset.
        /// </summary>
        public static readonly ImageManagerOptions Choose = new ImageManagerOptions(true, false, false);

        /// <summary>
        /// The image manager should allow browsing and deletion of image assets.
        /// </summary>
        public static readonly ImageManagerOptions Delete = new ImageManagerOptions(false, true, false);

        /// <summary>
        /// The image manager should allow browsing and importing of image assets.
        /// </summary>
        public static readonly ImageManagerOptions Import = new ImageManagerOptions(false, false, true);

        /// <summary>
        /// The image manager should allow browsing, deletion, importing and all editing actions for image assets.
        /// </summary>
        public static readonly ImageManagerOptions Edit = new ImageManagerOptions(false, true, true);

        /// <summary>
        /// The image manager should use it's default behaviour, equivalent to <see cref="Edit"/>.
        /// </summary>
        public static readonly ImageManagerOptions Default = new ImageManagerOptions(true, true, true);

        /// <summary>
        /// Should the image manager allow the user to choose an image asset?
        /// </summary>
        /// <value><c>true</c> if allow chooser; otherwise, <c>false</c>.</value>
        public bool AllowChooser { get; }

        /// <summary>
        /// Should the image manager allow the user to delete image assets?
        /// </summary>
        /// <value><c>true</c> if allow delete; otherwise, <c>false</c>.</value>
        public bool AllowDelete { get; }

        /// <summary>
        /// Should the image manager allow the user to import new image assets?
        /// </summary>
        /// <value><c>true</c> if allow import; otherwise, <c>false</c>.</value>
        public bool AllowImport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageManagerOptions"/> class.
        /// </summary>
        /// <param name="allowChooser">If set to <c>true</c> allow chooser.</param>
        /// <param name="allowDelete">If set to <c>true</c> allow delete.</param>
        /// <param name="allowImport">If set to <c>true</c> allow import.</param>
        public ImageManagerOptions(bool allowChooser,
                                   bool allowDelete,
                                   bool allowImport)
        {
            AllowChooser = allowChooser;
            AllowDelete = allowDelete;
            AllowImport = allowImport;
        }
    }
}
