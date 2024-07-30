using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using MFractor.IOC;
using MFractor.Logging;
using MFractor.Xml;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows.Utilities
{
    /// <summary>
    /// A helper class for caching the IDE name, version and installed extensions.
    /// </summary>
    static class IdeEnvironmentDetails
    {
        static readonly ILogger log = Logger.Create();

        static bool isPrepared = false;

        static string ExtensionsPath
        {
            get
            {
                var installationPath = GetAssemblyLocalPathFrom(typeof(MFractorPackage));
                var fileInfo = new FileInfo(installationPath);

                var directory = fileInfo.Directory;
                while (directory != null)
                {
                    if (directory.Name.Equals("Extensions", StringComparison.OrdinalIgnoreCase))
                    {
                        return directory.FullName;
                    }

                    directory = directory.Parent;
                }

                return string.Empty;
            }
        }

        public static void Prepare()
        {
            if (isPrepared)
            {
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                Assumes.Present(dte);

                IdeVersion = dte.Version;

                BuildExtensions();

                isPrepared = true;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        private static void BuildExtensions()
        {
            var extensionsPath = ExtensionsPath;

            if (!string.IsNullOrEmpty(extensionsPath) && Directory.Exists(extensionsPath))
            {
                var folders = Directory.EnumerateDirectories(extensionsPath);

                foreach (var folder in folders)
                {
                    var manifests = Directory.GetFiles(folder, ".vsixmanifest", SearchOption.AllDirectories);

                    if (manifests != null && manifests.Any())
                    {
                        foreach (var manifest in manifests)
                        {
                            if (File.Exists(manifest))
                            {
                                var ast = Resolver.Resolve<IXmlSyntaxParser>().ParseFile(manifest);

                                var identity = ast.Root.GetChildNode("Metadata")?.GetChildNode("Identity");

                                if (identity != null)
                                {
                                    var name = identity.GetAttributeByName("Id")?.Value?.Value;
                                    var version = identity.GetAttributeByName("Version")?.Value?.Value;

                                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(version))
                                    {
                                        extensions.Add(name + $" ({version})");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static string GetAssemblyLocalPathFrom(Type type)
        {
            var codebase = type.Assembly.CodeBase;
            var uri = new Uri(codebase, UriKind.Absolute);
            return uri.LocalPath;
        }

        readonly static List<string> extensions = new List<string>(); 

        public static IReadOnlyList<string> Extensions => extensions;

        public static string IdeVersion { get; private set; }
    }
}
