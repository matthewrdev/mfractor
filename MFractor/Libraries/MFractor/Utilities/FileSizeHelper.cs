using System;
using System.IO;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for pretty formatting a file size.
    /// </summary>
    public static class FileSizeHelper
    {
        /// <summary>
        /// The suffixes for various file sizes.
        /// </summary>
        public static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// For the given <paramref name="filePath"/>, get's its file size in bytes.
        /// </summary>
        /// <returns>The file size.</returns>
        /// <param name="filePath">File path.</param>
        public static long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return 0;
            }

            var f = new FileInfo(filePath);
            return f.Length;
        }

        /// <summary>
        /// Using the <paramref name="fileLengthBytes"/>, gets the formatted file size string.
        /// </summary>
        /// <returns>The formatted file size.</returns>
        /// <param name="fileLengthBytes">The length of the file in bytes.</param>
        public static string GetFormattedFileSize(long fileLengthBytes)
        {
            if (fileLengthBytes < 0) { return "-" + GetFormattedFileSize(-fileLengthBytes); }

            var i = 0;
            var dValue = (decimal)fileLengthBytes;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n1} {1}", dValue, SizeSuffixes[i]);
        }

        /// <summary>
        /// For the given file at <paramref name="filePath"/>, gets the formatted file size string.
        /// </summary>
        /// <returns>The formatted file size.</returns>
        /// <param name="filePath">File path.</param>
        public static string GetFormattedFileSize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return "NA";
            }

            var f = new FileInfo(filePath);

            return GetFormattedFileSize(f.Length);
        }

        /// <summary>
        /// Fro the given file at <paramref name="filePath"/>, gets the file size formatted in bytes.
        /// </summary>
        /// <returns>The raw file size.</returns>
        /// <param name="filePath">File path.</param>
        public static string GetRawFileSize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return "NA";
            }

            var f = new FileInfo(filePath);

            return f.Length + " bytes";
        }
    }
}
