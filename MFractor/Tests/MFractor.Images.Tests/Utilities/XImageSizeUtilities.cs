using System;
using System.Drawing.Imaging;
using System.IO;
using Image = SixLabors.ImageSharp.Image;
using Size = System.Drawing.Size;

namespace MFractor.Images.Tests.Utilities
{
    /// <summary>
    /// A cross-platform implementation of the IImageSizeUtilities using the ImageSharp library.
    /// </summary>
    class XImageSizeUtilities : IImageUtilities
    {
        public string GetImageFileExtension(string filename)
        {
            throw new NotImplementedException();
        }

        public ImageSize GetImageSize(string filename)
        {
            using var fileStream = File.OpenRead(filename);
            using var image = Image.Load(fileStream);
            return new ImageSize(image.Width, image.Height);
        }

        public string GetImageSizeSummary(string fileName)
        {
            throw new NotImplementedException();
        }

        public void ResizeImage(string imageFilePath, int width, int height, Stream destinationStream)
        {
            throw new NotImplementedException();
        }
    }
}
