using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
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
        sealed class CompilationXmlnsDefinitionCache
        {
            public IXmlnsDefinitionCollection Definitions { get; set; }
        }

        public class AssemblyNamespaceCollection
        {
            readonly Dictionary<string, NamespaceSymbolVisitor> namespaceCollection = new Dictionary<string, NamespaceSymbolVisitor>();

            internal INamespaceSymbol GetNamespaceByName(IAssemblySymbol assembly, string @namespace)
            {
                if (assembly is null || string.IsNullOrEmpty(@namespace))
                {
                    return null;
                }

                if (!namespaceCollection.TryGetValue(assembly.Name, out var walker))
                {
                    walker = new NamespaceSymbolVisitor();
                    walker.Visit(assembly);
                    namespaceCollection[assembly.Name] = walker;
                }

                return walker.GetNamespaceByName(@namespace);
            }
        }

        readonly ConditionalWeakTable<Compilation, CompilationXmlnsDefinitionCache> compilationCache = new ConditionalWeakTable<Compilation, CompilationXmlnsDefinitionCache>();

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

            var exportedXmlnsDefinitions = attributes
                .Where(a => SymbolHelper.DerivesFrom(a.AttributeClass, platform.XmlnsDefinitionAttribute.MetaType))
                .ToList();

            if (!exportedXmlnsDefinitions.Any())
            {
                return null;
            }

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

                var targetAssembly = assembly;
                if (namedArguments != null && namedArguments.Any(na => na.Key == "AssemblyName"))
                {
                    try
                    {
                        var value = namedArguments.FirstOrDefault(na => na.Key == "AssemblyName").Value;
                        var targetAssemblyName = value.Value as string;

                        if (!string.IsNullOrEmpty(targetAssemblyName))
                        {
                            targetAssembly = compilation.ResolveAssembly(targetAssemblyName) ?? targetAssembly;
                        }
                    }
                    catch
                    {
                    }
                }

                var ns = assemblyNamespaceCollection.GetNamespaceByName(targetAssembly, @namespace);

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

            return Resolve(project, compilation, platform);
        }

        public IXmlnsDefinitionCollection Resolve(Project project, Compilation compilation, IXamlPlatform platform)
        {
            if (project is null || compilation is null || platform is null)
            {
                return null;
            }

            var cacheBucket = compilationCache.GetValue(compilation, _ => new CompilationXmlnsDefinitionCache());
            lock (cacheBucket)
            {
                if (cacheBucket.Definitions != null)
                {
                    return cacheBucket.Definitions;
                }
            }

            var availableAssemblies = SymbolHelper.GetAllAvailableAssemblySymbols(project);
            var candidateAssemblies = availableAssemblies.Where(IsXmlnsDefinitionCandidate).ToList();

            var definitions = new Dictionary<string, HashSet<INamespaceSymbol>>();
            var collection = new AssemblyNamespaceCollection();

            foreach (var assembly in candidateAssemblies)
            {
                var assemblyDefinitions = Resolve(assembly, compilation, platform, collection);

                if (assemblyDefinitions == null || !assemblyDefinitions.Any())
                {
                    continue;
                }

                foreach (var definition in assemblyDefinitions)
                {
                    if (!definitions.TryGetValue(definition.Item1, out var namespaces))
                    {
                        namespaces = new HashSet<INamespaceSymbol>();
                        definitions[definition.Item1] = namespaces;
                    }

                    namespaces.Add(definition.Item2);
                }
            }

            var xmlnsDefinitions = new List<IXmlnsDefinition>();
            foreach (var definition in definitions)
            {
                xmlnsDefinitions.Add(new XmlnsDefinition(definition.Key, definition.Value.ToList()));
            }

            var resolvedDefinitions = new XmlnsDefinitionCollection(xmlnsDefinitions);
            lock (cacheBucket)
            {
                cacheBucket.Definitions = resolvedDefinitions;
            }

            return resolvedDefinitions;
        }

        public bool IsXmlnsDefinitionCandidate(IAssemblySymbol assembly)
        {
            if (assembly is null)
            {
                return false;
            }

            var assemblyName = assembly.Name;

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
