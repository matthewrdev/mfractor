using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MFractor.IOC;

namespace MFractor.Rider
{
    public class MFractorRiderExportResolver : AssemblyCompositionExportResolver
    {
        public override IEnumerable<Assembly> Assemblies
        {
            get
            {
                var assemblyDirectory = Path.GetDirectoryName(typeof(MFractorRiderExportResolver).Assembly.Location);
                var assemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

                void register(Assembly assembly)
                {
                    if (assembly == null || string.IsNullOrEmpty(assembly.FullName))
                    {
                        return;
                    }

                    assemblies[assembly.FullName] = assembly;
                }

                register(typeof(MFractorRiderExportResolver).Assembly);

                if (string.IsNullOrEmpty(assemblyDirectory) || !Directory.Exists(assemblyDirectory))
                {
                    return assemblies.Values;
                }

                foreach (var filePath in Directory.EnumerateFiles(assemblyDirectory, "MFractor*.dll"))
                {
                    var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
                    {
                        try
                        {
                            return string.Equals(a.Location, filePath, StringComparison.OrdinalIgnoreCase);
                        }
                        catch
                        {
                            return false;
                        }
                    });

                    if (loadedAssembly != null)
                    {
                        register(loadedAssembly);
                        continue;
                    }

                    try
                    {
                        register(Assembly.LoadFrom(filePath));
                    }
                    catch
                    {
                    }
                }

                return assemblies.Values;
            }
        }

        protected override void RegisterExternalParts(IExternalPartRegistrar registrar)
        {
        }
    }
}
