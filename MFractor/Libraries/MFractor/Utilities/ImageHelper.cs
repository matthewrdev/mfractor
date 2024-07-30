using System;
using System.Collections.Generic;
using System.IO;

namespace MFractor.Utilities
{
    /// <summary>
    /// The image helper is used to resize images, get image dimensions, convert images between formats and more.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// The image extensions.
        /// </summary>
        public static readonly HashSet<string> ImageExtensions = new HashSet<string> { "jpg", "jpeg", "png", "gif", "svg" };

        /// <summary>
        /// The image extensions.
        /// </summary>
        public static readonly HashSet<string> BitmapImageExtensions = new HashSet<string> { "jpg", "jpeg", "png" };

        /// <summary>
        /// Is the provided <paramref name="imageFileName"/> an image file?
        /// </summary>
        /// <returns><c>true</c>, if image file was ised, <c>false</c> otherwise.</returns>
        /// <param name="imageFileName">Image file name.</param>
        public static bool IsImageFile(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName))
            {
                return false;
            }

            if (!imageFileName.Contains("."))
            {
                // No file extension.
                return false;
            }

            var extension = Path.GetExtension(imageFileName);

            return IsImageFileExtension(extension);
        }

        /// <summary>
        /// Is the provided <paramref name="imageFileName"/> an image file?
        /// </summary>
        /// <returns><c>true</c>, if image file was ised, <c>false</c> otherwise.</returns>
        /// <param name="imageFileName">Image file name.</param>
        public static bool IsBitmapImageFile(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName))
            {
                return false;
            }

            if (!imageFileName.Contains("."))
            {
                // No file extension.
                return false;
            }

            var extension = Path.GetExtension(imageFileName);

            return IsBitmapImageFileExtension(extension);
        }

        /// <summary>
        /// Is the provided <paramref name="fileExtension"/> an image file extension?
        /// </summary>
        /// <returns><c>true</c>, if image file extension was ised, <c>false</c> otherwise.</returns>
        /// <param name="fileExtension">File extension.</param>
        public static bool IsImageFileExtension(string fileExtension)
        {
            var extension = fileExtension.StartsWith(".", StringComparison.OrdinalIgnoreCase) ? fileExtension.Remove(0, 1) : fileExtension;

            return ImageExtensions.Contains(extension.ToLower());
        }

        public static bool IsBitmapImageFileExtension(string fileExtension)
        {
            return IsImageFileExtension(fileExtension, BitmapImageExtensions);
        }

        /// <summary>
        /// Is the provided <paramref name="fileExtension"/> an image file extension?
        /// </summary>
        public static bool IsImageFileExtension(string fileExtension, HashSet<string> candidateExtensions)
        {
            if (candidateExtensions is null)
            {
                return false;
            }

            var extension = fileExtension.StartsWith(".", StringComparison.OrdinalIgnoreCase) ? fileExtension.Remove(0, 1) : fileExtension;

            return candidateExtensions.Contains(extension.ToLower());
        }


        public static ImageSize ResizeKeepAspect(this ImageSize size, int maxWidth, int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, size.Width);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, size.Height);

            var ratio = Math.Min(maxWidth / (decimal)size.Width, maxHeight / (decimal)size.Height);
            return new ImageSize((int)Math.Round(size.Width * ratio), (int)Math.Round(size.Height * ratio));
        }
    }
}

