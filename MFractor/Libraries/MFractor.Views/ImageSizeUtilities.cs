using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MFractor.Views
{
    [Export(typeof(IImageUtilities))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ImageSizeUtilities : IImageUtilities
    {
        readonly Lazy<IPlatformService> platformService;
        IPlatformService PlatformService => platformService.Value;

        [ImportingConstructor]
        public ImageSizeUtilities(Lazy<IPlatformService> platformService)
        {
            this.platformService = platformService;
        }

        public ImageSize GetImageSize(string filename)
        {
            if (!File.Exists(filename))
            {
                return default;
            }

            using (var image = Xwt.Drawing.Image.FromFile(filename))
            {
                return new ImageSize((int)image.Size.Width, (int)image.Size.Height);
            }
        }

        public string GetImageSizeSummary(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    var imageSize = GetImageSize(fileName);
                    return "Width: " + imageSize.Width.ToString() + " | " + "Height: " + imageSize.Height.ToString();
                }
            }
            catch
            {
            }

            return "Width: NA | Height: NA";
        }

        public void ResizeImage(string imageFilePath, int width, int height, Stream destinationStream)
        {
            if (string.IsNullOrWhiteSpace(imageFilePath))
            {
                throw new ArgumentException($"'{nameof(imageFilePath)}' cannot be null or whitespace.", nameof(imageFilePath));
            }

            if (destinationStream is null)
            {
                throw new ArgumentNullException(nameof(destinationStream));
            }

            if (PlatformService.IsWindows)
            {
                ResizeImage_Windows(imageFilePath, width, height, destinationStream, ImageFormat.Png);
            }
            else
            {

                ResizeImage_MacOS(imageFilePath, width, height, destinationStream);
            }
        }

        private void ResizeImage_MacOS(string imageFilePath, int width, int height, Stream destinationStream)
        {
#if VS_MAC
            using (var stream = File.OpenRead(imageFilePath))
            {
                using (var bitmap = SkiaSharp.SKBitmap.Decode(stream))
                {
                    var imageInfo = new SkiaSharp.SKImageInfo(width, height);
                    using (var resizedBitmap = bitmap.Resize(imageInfo, SkiaSharp.SKFilterQuality.High))
                    {
                        resizedBitmap.Encode(destinationStream, SkiaSharp.SKEncodedImageFormat.Png, 100);
                    }
                }
            }
#endif
        }

        private void ResizeImage_Windows(string imageFilePath, int width, int height, Stream destinationStream, ImageFormat imageFormat)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using (var image = Image.FromFile(imageFilePath))
            {
                    //a holder for the result
                    var bmp = new Bitmap(width, height);

                    if (Math.Abs(image.HorizontalResolution) > float.Epsilon
                        && Math.Abs(image.VerticalResolution) > float.Epsilon)
                    {
                        //set the resolutions the same to avoid cropping due to resolution differences
                        bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    }

                    //use a graphics object to draw the resized image into the bitmap
                    using (var graphics = Graphics.FromImage(bmp))
                    {
                        //set the resize quality modes to high quality
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        //draw the image into the target bitmap
                        graphics.DrawImage(image, 0, 0, bmp.Width, bmp.Height);
                    }

                bmp.Save(destinationStream, imageFormat);
            }
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public string GetImageFileExtension(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
