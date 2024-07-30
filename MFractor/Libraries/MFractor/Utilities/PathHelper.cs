using System;
using System.IO;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for working with file paths.
    /// </summary>
    public static class PathHelper
    {
        public static string CorrectDirectorySeparatorsInPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            var oldSeparator = "\\";
            var newSeparator = "/";
            if (Path.DirectorySeparatorChar == '\\')
            {
                oldSeparator = "/";
                newSeparator = "\\";
            }

            return filePath.Replace(oldSeparator, newSeparator);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativePath(string fromPath, string toPath)
		{
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentException("message", nameof(fromPath));
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentException("message", nameof(toPath));
            }

            var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (toUri.Scheme.ToUpperInvariant() == "FILE")
			{
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}

        /// <summary>
        /// Gets the absolute path.
        /// <para/>
        /// See https://stackoverflow.com/a/35218619/1099111
        /// </summary>
        /// <returns>The absolute path.</returns>
        /// <param name="path">Path.</param>
        public static String GetAbsolutePath(String path)
        {
            return GetAbsolutePath(null, path);
        }

        /// <summary>
        /// Gets the absolute path.
        /// <para/>
        /// See https://stackoverflow.com/a/35218619/1099111
        /// </summary>
        /// <returns>The absolute path.</returns>
        /// <param name="basePath">Base path.</param>
        /// <param name="path">Path.</param>
        public static string GetAbsolutePath(String basePath, String path)
        {
            if (path == null)
            {
                return null;
            }

            if (basePath == null)
            {
                basePath = Path.GetFullPath("."); // quick way of getting current working directory
            }
            else
            {
                basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)
            }

            string finalPath;
            // specific for windows paths starting on \ - they need the drive added to them.
            // I constructed this piece like this for possible Mono support.
            if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
            {
                if (path.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
                }
                else
                {
                    finalPath = Path.Combine(basePath, path);
                }
            }
            else
            {
                finalPath = path;
            }

            // resolves any internal "..\" to get the true full path.
            return Path.GetFullPath(finalPath);
        }

        /// <summary>
        /// Checks if the provided <paramref name="folder"/> is within the <paramref name="path"/>.
        /// </summary>
        /// <returns><c>true</c>, if inside was ised, <c>false</c> otherwise.</returns>
        /// <param name="folder">Folder.</param>
        /// <param name="path">Path.</param>
        public static bool IsInside(string folder, string path)
		{ 
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.IndexOf (folder, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}
}

