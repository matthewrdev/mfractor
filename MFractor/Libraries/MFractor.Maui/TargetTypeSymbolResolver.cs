using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Symbols;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITargetTypeSymbolResolver))]
    class TargetTypeSymbolResolver : ITargetTypeSymbolResolver
    {
        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        [ImportingConstructor]
        public TargetTypeSymbolResolver(Lazy<IXamlTypeResolver> xamlTypeResolver)
        {
            this.xamlTypeResolver = xamlTypeResolver;
        }

        public TargetTypeSymbolResult GetTargetTypeSymbolForNode(XmlNode node,
                                                                 Project project,
                                                                 IXamlNamespaceCollection namespaces,
                                                                 IXmlnsDefinitionCollection xmlnsDefinitions,
                                                                 string attributeName,
                                                                 bool searchParents)
        {
            if (node == null)
            {
                return TargetTypeSymbolResult.Failure;
            }

            var targetTypeAttribute = node.GetAttributeByName(attributeName);

            if (targetTypeAttribute == null)
            {
                if (!searchParents)
                {
                    return TargetTypeSymbolResult.Failure;
                }

                return GetTargetTypeSymbolForNode(node.Parent, project, namespaces, xmlnsDefinitions, attributeName, searchParents);
            }

            var value = targetTypeAttribute.Value?.Value;
            if (string.IsNullOrEmpty(value))
            {
                return TargetTypeSymbolResult.Failure;
            }

            var namespaceName = "";
            var typeName = value;
            if (value.Contains(":"))
            {
                var components = value.Split(':');
                if (components.Length < 2)
                {
                    return TargetTypeSymbolResult.Failure;
                }

                namespaceName = components[0];
                typeName = components[1];
            }

            var xmlns = namespaces.ResolveNamespace(namespaceName);

            var type = XamlTypeResolver.ResolveType(typeName, xmlns, project, xmlnsDefinitions);

            return new TargetTypeSymbolResult(type, xmlns);
        }
    }
}