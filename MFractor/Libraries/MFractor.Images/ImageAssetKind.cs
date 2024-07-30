namespace MFractor.Images
{
    /// <summary>
    /// The kind of image asset.
    /// </summary>
    public enum ImageAssetKind
    {
        /// <summary>
        /// The <see cref="IImageAsset"/> is an image resource.
        /// </summary>
        Resource,

        /// <summary>
        /// The <see cref="IImageAsset"/> is a package application icon.
        /// </summary>
        AppIcon,

        /// <summary>
        /// The <see cref="IImageAsset"/> is an artwork asset.
        /// </summary>
        Artwork,
    }
}