namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// Options to specify the behaviour of the image manager.
    /// </summary>
    public interface IImageManagerOptions
    {
        /// <summary>
        /// Should the image manager allow the user to choose an image asset?
        /// </summary>
        /// <value><c>true</c> if allow chooser; otherwise, <c>false</c>.</value>
        bool AllowChooser { get; }

        /// <summary>
        /// Should the image manager allow the user to delete image assets?
        /// </summary>
        /// <value><c>true</c> if allow delete; otherwise, <c>false</c>.</value>
        bool AllowDelete { get; }

        /// <summary>
        /// Should the image manager allow the user to import new image assets?
        /// </summary>
        /// <value><c>true</c> if allow import; otherwise, <c>false</c>.</value>
        bool AllowImport { get; }
    }
}