using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlnsNamespaceSymbolResolver))]
    public class XmlnsNamespaceSymbolResolver : IXmlnsNamespaceSymbolResolver
    {
        readonly Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver;
        public IXmlnsDefinitionResolver XmlnsDefinitionResolver => xmlnsDefinitionResolver.Value;

        [ImportingConstructor]
        public XmlnsNamespaceSymbolResolver(Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver)
        {
            this.xmlnsDefinitionResolver = xmlnsDefinitionResolver;
        }

        public IEnumerable<INamespaceSymbol> GetNamespaces(IXamlSchema xamlSchema, Project project, IXamlPlatform platform)
        {
            if (xamlSchema is null || project is null || platform is null)
            {
                return Enumerable.Empty<INamespaceSymbol>();
            }

            var definitions = XmlnsDefinitionResolver.Resolve(project, platform);

            return GetNamespaces(xamlSchema, definitions);
        }

        public IEnumerable<INamespaceSymbol> GetNamespaces(IXamlSchema xamlSchema, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (xamlSchema is null || xmlnsDefinitions is null)
            {
                return Enumerable.Empty<INamespaceSymbol>();
            }

            var definition = xmlnsDefinitions.GetDefinitionForUri(xamlSchema.Uri);

            if (definition is null)
            {
                return Enumerable.Empty<INamespaceSymbol>();
            }

            return definition.Namespaces;
        }

        public IEnumerable<INamespaceSymbol> GetNamespaces(IXamlNamespace xamlNamespace, Project project, IXamlPlatform platform)
        {
            if (xamlNamespace is null || project is null)
            {
                return Enumerable.Empty<INamespaceSymbol>();
            }

            var definitions = XmlnsDefinitionResolver.Resolve(project, platform);

            return GetNamespaces(xamlNamespace, project, definitions);
        }

        public IEnumerable<INamespaceSymbol> GetNamespaces(IXamlNamespace xamlNamespace, Project project, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (xamlNamespace is null || xmlnsDefinitions is null || project is null)
            {
                return Enumerable.Empty<INamespaceSymbol>();
            }

            if (xamlNamespace.Schema != null)
            {
                return GetNamespaces(xamlNamespace.Schema, xmlnsDefinitions);
            }

            var namespaceSymbol = GetNamespaceSymbol(xamlNamespace, project);
            
            if (namespaceSymbol is null)
            {
                return Enumerable.Empty<INamespaceSymbol>();
            }

            return namespaceSymbol.AsList();
        }

        INamespaceSymbol GetNamespaceSymbol(IXamlNamespace xamlNamespace, Project project)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var namespaceCompilation = compilation;

            var assemblyName = xamlNamespace.AssemblyComponent?.AssemblyName;
            var platformName = xamlNamespace.TargetPlatformComponent?.TargettedPlatform;
            var namespaceName = xamlNamespace.ClrNamespaceComponent?.Namespace;

            IAssemblySymbol assemblySymbol = null;

            if (!string.IsNullOrEmpty(assemblyName))
            {
                if (string.IsNullOrEmpty(platformName))
                {
                    assemblySymbol = SymbolHelper.ResolveAssembly(compilation, assemblyName);
                }
                else
                {
                    var expectedAssembly = "";
                    if (platformName.Equals("android", StringComparison.OrdinalIgnoreCase))
                    {
                        expectedAssembly = "Mono.Android";
                    }
                    else if (platformName.Equals("ios", StringComparison.OrdinalIgnoreCase))
                    {
                        expectedAssembly = "Xamarin.iOS";
                    } // TODO: This needs to be updated to support MAUI.

                    var projects = project.Solution.Projects.Where(p => p.MetadataReferences.Any(md => System.IO.Path.GetFileNameWithoutExtension(new System.IO.FileInfo(md.Display).Name) == expectedAssembly)).ToList();

                    foreach (var p in projects)
                    {
                        if (!p.TryGetCompilation(out var projectCompilation))
                        {
                            continue;
                        }

                        var symbol = SymbolHelper.ResolveAssembly(projectCompilation, assemblyName);

                        if (symbol != null)
                        {
                            assemblySymbol = symbol;
                            namespaceCompilation = projectCompilation;
                            break;
                        }
                    }
                }
            }
            else
            {
                assemblyName = compilation.AssemblyName;
                assemblySymbol = compilation.Assembly;
            }

            return SymbolHelper.ResolveNamespaceSymbol(namespaceName, assemblySymbol);
        }

        public IEnumerable<IAssemblySymbol> GetAssemblies(IXamlSchema xamlSchema, Project project, IXamlPlatform platform)
        {
            if (xamlSchema is null
                || project is null
                || platform is null)
            {
                return null;
            }

            var definitions = XmlnsDefinitionResolver.Resolve(project, platform);

            return GetAssemblies(xamlSchema, definitions);
        }

        public IEnumerable<IAssemblySymbol> GetAssemblies(IXamlSchema xamlSchema, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (xamlSchema is null
                || xmlnsDefinitions is null)
            {
                return null;
            }

            var definition = xmlnsDefinitions.GetDefinitionForUri(xamlSchema.Uri);

            if (definition is null)
            {
                return null;
            }

            return definition.Assemblies;
        }

        public IEnumerable<IAssemblySymbol> GetAssemblies(IXamlNamespace xamlNamespace, Project project, IXamlPlatform platform)
        {
            if (xamlNamespace is null
                || project is null
                || platform is null)
            {
                return null;
            }

            var definitions = XmlnsDefinitionResolver.Resolve(project, platform);

            return GetAssemblies(xamlNamespace, project, definitions);
        }

        public IEnumerable<IAssemblySymbol> GetAssemblies(IXamlNamespace xamlNamespace, Project project, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (xamlNamespace is null
                || project is null
                || xmlnsDefinitions is null)
            {
                return null;
            }

            if (xamlNamespace.IsSchema)
            {
                return GetAssemblies(xamlNamespace.Schema, xmlnsDefinitions);
            }

            return GetNamespaceSymbol(xamlNamespace, project)?.ContainingAssembly?.AsList();
        }
    }
}