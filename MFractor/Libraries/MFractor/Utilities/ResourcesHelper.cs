using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MFractor.Utilities
{
    /// <summary>
    /// Helper class for locating and reading embedded resources in an assembly.
    /// </summary>
    public static class ResourcesHelper
    {
        /// <summary>
        /// Locates a resource in the provided assembly that has a partial match with the provided name.
        /// </summary>
        /// <returns>The matching resource identifier.</returns>
        /// <param name="instance">The instance.</param>
        /// <param name="partialName">Partial name.</param>
        /// <param name="suffix">Suffix.</param>
        public static string LocateMatchingResourceId(object instance, string partialName, string suffix = "")
        {
            if (instance == null)
            {
                return string.Empty;
            }

            return LocateMatchingResourceId(instance.GetType().Assembly, partialName, suffix);
        }

        /// <summary>
        /// Locates a resource in the provided assembly that has a partial match with the provided name.
        /// </summary>
        /// <returns>The matching resource identifier.</returns>
        /// <param name="assembly">Assembly.</param>
        /// <param name="partialName">Partial name.</param>
        /// <param name="suffix">Suffix.</param>
		public static string LocateMatchingResourceId(Assembly assembly, string partialName, string suffix = "")
		{
            var resourceName = partialName;

			if (string.IsNullOrEmpty(suffix) == false)
			{
				resourceName += suffix;
			}

            var resources = assembly.GetManifestResourceNames();

			var match = resources.FirstOrDefault(s => s.Contains(resourceName));

            if (match == null)
            {
                //Debugger.Break();
            }

			return match;
		}

        /// <summary>
        /// Reads the content of the resource from the assembly that owns <paramref name="instance"/>.
        /// </summary>
        /// <returns>The resource content.</returns>
        /// <param name="instance">Assembly.</param>
        /// <param name="resourceName">Resource name.</param>
        public static string ReadResourceContent(object instance, string resourceName)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            return ReadResourceContent(instance.GetType().Assembly, resourceName);
        }

        /// <summary>
        /// Reads the content of the resource from the provided assembly.
        /// </summary>
        /// <returns>The resource content.</returns>
        /// <param name="assembly">Assembly.</param>
        /// <param name="resourceName">Resource name.</param>
		public static string ReadResourceContent(Assembly assembly, string resourceName)
		{
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

        /// <summary>
        /// Reads the <paramref name="resourceName"/> content in the <paramref name="assembly"/> and writes it to disk.
        /// </summary>
        /// <param name="assembly">The assembly that the <paramref name="resourceName"/> is located.</param>
        /// <param name="resourceName">The embedded resource asset to export.</param>
        /// <param name="filePath">The file path on disk to export the <paramref name="resourceName"/> to.</param>
        public static void ExportResourceContentToFile(Assembly assembly, string resourceName, string filePath)
        {
            using (var resource = assembly.GetManifestResourceStream(resourceName))
            {
                using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
    }
}
