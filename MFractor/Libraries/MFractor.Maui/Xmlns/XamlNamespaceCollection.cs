using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MFractor.Maui.Syntax;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    class XamlNamespaceCollection : IXamlNamespaceCollection
    {
        public IReadOnlyList<IXamlNamespace> Namespaces { get; private set; }

        public IReadOnlyDictionary<string, IXamlNamespace> IndexedByPrefix { get; }
        public IReadOnlyDictionary<string, IXamlNamespace> IndexedBySchemaUri { get; }
        public IReadOnlyDictionary<string, IXamlNamespace> IndexedByClrNamespaceName { get; }
        public IReadOnlyDictionary<string, IXamlNamespace> IndexedByValue { get; }

        public bool HasDefaultNamespace => DefaultNamespace != null;
        public IXamlNamespace DefaultNamespace { get; }

        public XamlNamespaceCollection(IEnumerable<IXamlNamespace> namespaces)
        {
            this.Namespaces = (namespaces ?? Enumerable.Empty<IXamlNamespace>()).ToList();

            var indexedByPrefix = new Dictionary<string, IXamlNamespace>();
            var indexedBySchemaUri = new Dictionary<string, IXamlNamespace>();
            var indexedByValue = new Dictionary<string, IXamlNamespace>();
            var indexedByClrNamespaceName = new Dictionary<string, IXamlNamespace>();

            foreach (var xmlns in Namespaces)
            {
                if (xmlns is null || xmlns.Prefix == null)
                {
                    continue;
                }

                indexedByPrefix[xmlns.Prefix] = xmlns;
                if (xmlns.Schema != null)
                {
                    indexedBySchemaUri[xmlns.Schema.Uri] = xmlns;
                }
                if (xmlns.HasClrNamespaceContent)
                {
                    indexedByClrNamespaceName[xmlns.ClrNamespaceComponent.Namespace] = xmlns;
                }

                if (xmlns.HasValue)
                {
                    indexedByValue[xmlns.Value] = xmlns;
                }
            }

            IndexedByPrefix = indexedByPrefix;
            IndexedBySchemaUri = indexedBySchemaUri;
            IndexedByClrNamespaceName = indexedByClrNamespaceName;
            IndexedByValue = indexedByValue;

            if (IndexedByPrefix.TryGetValue(string.Empty, out var @default))
            {
                DefaultNamespace = @default; ;
            }
        }

        public IXamlNamespace ResolveNamespace(XmlNode node)
        {
            return ResolveNamespace(node.Name.Namespace);
        }

        public IXamlNamespace ResolveNamespace(XmlAttribute attribute)
        {
            return ResolveNamespace(attribute.Name.Namespace);
        }

        public IXamlNamespace ResolveNamespace(NameSyntax syntaxNode)
        {
            if (syntaxNode is SymbolSyntax symbol)
            {
                return ResolveNamespace(symbol.NamespaceSyntax?.Namespace);
            }
            else if (syntaxNode is TypeNameSyntax typeName)
            {
                if (typeName.Parent is SymbolSyntax parent)
                {
                    return ResolveNamespace(parent);
                }

                return ResolveNamespace(string.Empty);
            }
            else if (syntaxNode is NamespaceSyntax @namespace)
            {
                return ResolveNamespace(@namespace.Namespace);
            }

            return null;
        }

        public IXamlNamespace ResolveNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return DefaultNamespace;
            }

            if (IndexedByPrefix.TryGetValue(namespaceName, out var result))
            {
                return result;
            }

            return default;
        }

        public IXamlNamespace ResolveNamespaceForSchema(IXamlSchema schema)
        {
            return ResolveNamespaceForSchema(schema.Uri);
        }

        /// <summary>
        /// Resolves the <see cref="XamlNamespace"/> that matches the provided <paramref name="schemaUrl"/>.
        /// </summary>
        /// <returns>The namespace for schema.</returns>
        /// <param name="schemaUrl">Schema URL.</param>
        public IXamlNamespace ResolveNamespaceForSchema(string schemaUrl)
        {
            if (IndexedBySchemaUri.TryGetValue(schemaUrl, out var result))
            {
                return result;
            }

            return default;
        }

        public IEnumerator<IXamlNamespace> GetEnumerator()
        {
            return Namespaces.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Namespaces.GetEnumerator();
        }
    }
}
