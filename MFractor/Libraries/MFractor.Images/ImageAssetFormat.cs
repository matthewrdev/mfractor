namespace MFractor.Images
{
    public enum ImageAssetFormat
    {
        /// <summary>
        /// The image asset is a bitmap image.
        /// <para/>
        /// For example, it is a 
        /// </summary>
        Bitmap,

        /// <summary>
        /// The image asset is a PDF resource that generates, at build time, a series of dependant bitmap artifacts for the native platform.
        /// </summary>
        Pdf,

        /// <summary>
        /// The image asset is vector based image resource such as an SVG.
        /// </summary>
        Vector,
    }
}