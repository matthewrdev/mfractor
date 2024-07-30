using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MFractor.Attributes;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for working with assemblies.
    /// </summary>
	public static class AssemblyHelper
	{
        /// <summary>
        /// Gets the entry assembly for the program.
        /// </summary>
        /// <value>The entry assembly.</value>
        public static Assembly EntryAssembly => Assembly.GetEntryAssembly();

        /// <summary>
        /// Gets the directory of the entry assembly.
        /// </summary>
        /// <value>The entry assembly directory.</value>
        public static string EntryAssemblyDirectory => DirectoryForAssembly(EntryAssembly);

        /// <summary>
        /// For the given <paramref name="assembly"/>, gets the directory it resides within.
        /// </summary>
        /// <returns>The for assembly.</returns>
        /// <param name="assembly">Assembly.</param>
        public static string DirectoryForAssembly(Assembly assembly)
		{
			var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}

        /// <summary>
        /// Finds the <see cref="Assembly"/> in the app domain that is named "<paramref name="name"/>".
        /// </summary>
        /// <returns>The assembly by name.</returns>
        /// <param name="name">Name.</param>
		public static Assembly GetAssemblyByName(string name)
		{
			return AppDomain.CurrentDomain.GetAssemblies().
				   SingleOrDefault(assembly => assembly.GetName().Name == name);
		}

        /// <summary>
        /// For the given <paramref name="assembly"/>, finds the types that can be loaded.
        /// </summary>
        /// <returns>The loadable types.</returns>
        /// <param name="assembly">Assembly.</param>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
	}
}

