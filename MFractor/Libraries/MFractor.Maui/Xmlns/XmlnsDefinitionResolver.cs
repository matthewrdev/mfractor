using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Utilities.SymbolVisitors;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlnsDefinitionResolver))]
    class XmlnsDefinitionResolver : IXmlnsDefinitionResolver
    {
        public class AssemblyNamespaceCollection
        {
            readonly Dictionary<string, NamespaceSymbolVisitor> namespaceCollection = new Dictionary<string, NamespaceSymbolVisitor>();

            internal INamespaceSymbol GetNamespaceByName(IAssemblySymbol assembly, string @namespace)
            {
                if (assembly is null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(@namespace))
                {
                    return null;
                }

                if (!namespaceCollection.TryGetValue(assembly.Name, out var walker))
                {
                    walker = new NamespaceSymbolVisitor();
                    walker.Visit(assembly);
                }

                return walker.GetNamespaceByName(@namespace);
            }
        }

        readonly HashSet<string> excludedXmlnsAssemblyNames = new HashSet<string>()
        {
            "Newtonsoft.Json",
            "mscorelib",
            "netstandard",
        };

        public IReadOnlyList<string> ExcludedXmlnsAssemblyNames => excludedXmlnsAssemblyNames.ToList();

        public List<(string, INamespaceSymbol)> Resolve(IAssemblySymbol assembly,
                                                        Compilation compilation,
                                                        IXamlPlatform platform,
                                                        AssemblyNamespaceCollection assemblyNamespaceCollection)
        {
            if (assembly is null)
            {
                return null;
            }

            var attributes = assembly.GetAttributes();

            if (attributes == null || !attributes.Any())
            {
                return null;
            }

            var exportedXmlnsDefinitions = attributes.Where(a => SymbolHelper.DerivesFrom(a.AttributeClass, platform.XmlnsDefinitionAttribute.MetaType)).ToList();

            if (!exportedXmlnsDefinitions.Any())
            {
                return null;
            }

            var targetAssembly = assembly;
            var result = new List<(string, INamespaceSymbol)>();
            foreach (var xmlns in exportedXmlnsDefinitions)
            {
                var arguments = xmlns.ConstructorArguments;
                var namedArguments = xmlns.NamedArguments;

                if (arguments == null || arguments.Length < 2)
                {
                    continue;
                }

                var uri = arguments[0].Value as string;
                var @namespace = arguments[1].Value as string;

                if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(@namespace))
                {
                    continue;
                }

                if (namedArguments != null && namedArguments.Any(na => na.Key == "AssemblyName"))
                {
                    try
                    {
                        var value = namedArguments.FirstOrDefault(na => na.Key == "AssemblyName").Value;
                        var tempName = value.Type.ContainingAssembly.Name;

                        assembly = compilation.ResolveAssembly(tempName);
                    }
                    catch { }
                }

                var ns = assemblyNamespaceCollection.GetNamespaceByName(assembly, @namespace);

                if (ns != null)
                {
                    result.Add((uri, ns));
                }
            }

            return result;
        }

        public IXmlnsDefinitionCollection Resolve(Project project, IXamlPlatform platform)
        {
            if (project is null || platform is null)
            {
                return null;
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var availableAssemblies = SymbolHelper.GetAllAvailableAssemblySymbols(project);

            var definitions = new Dictionary<string, HashSet<INamespaceSymbol>>();

            var collection = new AssemblyNamespaceCollection();

            var candidateAssemblies = availableAssemblies.Where(IsXmlnsDefinitionCandidate).ToList();

            foreach (var assembly in availableAssemblies)
            {
                var assemblyDefinitions = Resolve(assembly, compilation, platform, collection);

                if (assemblyDefinitions != null && assemblyDefinitions.Any())
                {
                    foreach (var definition in assemblyDefinitions)
                    {
                        if (!definitions.ContainsKey(definition.Item1))
                        {
                            definitions[definition.Item1] = new HashSet<INamespaceSymbol>();
                        }

                        definitions[definition.Item1].Add(definition.Item2);
                    }
                }
            }

            var xmlnsDefinitions = new List<IXmlnsDefinition>();

            foreach (var d in definitions)
            {
                xmlnsDefinitions.Add(new XmlnsDefinition(d.Key, d.Value.ToList()));
            }

            return new XmlnsDefinitionCollection(xmlnsDefinitions);
        }

        public bool IsXmlnsDefinitionCandidate(IAssemblySymbol assembly)
        {
            if (assembly is null)
            {
                return false;
            }

            var assemblyName = assembly.Name;

            // Exclude known assemblies that definitely do not have a xmlns attribute.
            if (assemblyName.StartsWith("System")
                || assemblyName.StartsWith("Microsoft.Extensions")
                || assemblyName.StartsWith("mscorlib")
                || assemblyName.StartsWith("Microsoft.Win32")
                || assemblyName.StartsWith("Microsoft.CSharp")
                || assemblyName.StartsWith("Microsoft.VisualBasic")
                || assemblyName.StartsWith("SQLite"))
            {
                return false;
            }

            return !excludedXmlnsAssemblyNames.Contains(assemblyName);
        }
    }
}