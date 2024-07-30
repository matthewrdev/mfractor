namespace MFractor.Images
{
    /// <summary>
    /// The kind of refere
    /// </summary>
    public enum ImageAssetUsageKind
    {
        /// <summary>
        /// The detected usage of the image asset is definite.
        /// <para/>
        /// Example 1: An Android "@drawable/myImage" is a strong reference.
        /// <para/>
        /// Example 2: A XAML property setter that is an ImageSource and takes a string literal input like "myImage" is a strong reference.
        /// <para/>
        /// Example 3: An iOS UIImage.FromBundle("myImage") is a strong reference.
        /// </summary>
        StrongReference,

        /// <summary>
        /// A likely usage of the image asset, however, there is room for ambiguity.
        /// <para/>
        /// Example 1: A method that returns a string constant like "myImage.png" would be considered a weak reference.
        /// <para/>
        /// Example 2: A XAML on platform that that returns "myImage.png" would be considered a weak reference.
        /// </summary>
        WeakReference,

        /// <summary>
        /// The usage of this image asset is a form of declaration.
        /// <para/>
        /// For example, the Android.Drawable.myImage declaration in the Resources.designer.cs file.
        /// </summary>
        Declaration,
    }
}
