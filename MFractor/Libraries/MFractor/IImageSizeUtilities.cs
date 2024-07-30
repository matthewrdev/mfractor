using System;
using System.IO;

namespace MFractor
{
    public interface IImageUtilities
    {
        /// <summary>
        /// Gets the dimensions of an image.
        /// </summary>
        /// <param name="filename">The path of the image to get the dimensions of.</param>
        /// <returns>The dimensions of the specified image.</returns>
        /// <exception cref="ArgumentException">The image was of an unrecognized format.</exception>
        ImageSize GetImageSize(string filename);

        string GetImageFileExtension(string filename);

        /// <summary>
        /// Gets a summary of the dimensions of the provided image.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string GetImageSizeSummary(string fileName);

        /// <summary>
        /// Resize the image to the specified width and height.
        /// <para/>
        /// The resized image will always be in png format.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        void ResizeImage(string imageFilePath, int width, int height, Stream destinationStream);
    }
}
