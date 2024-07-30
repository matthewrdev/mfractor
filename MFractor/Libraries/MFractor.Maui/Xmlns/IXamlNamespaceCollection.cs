using System.Collections.Generic;
using MFractor.Maui.Syntax;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    public interface IXamlNamespaceCollection : IEnumerable<IXamlNamespace>
    {
        IReadOnlyList<IXamlNamespace> Namespaces { get; }

        IXamlNamespace DefaultNamespace { get; }
        bool HasDefaultNamespace { get; }

        IXamlNamespace ResolveNamespace(XmlNode node);
        IXamlNamespace ResolveNamespace(XmlAttribute attribute);
        IXamlNamespace ResolveNamespace(NameSyntax syntaxNode);
        IXamlNamespace ResolveNamespace(string namespaceName);
        IXamlNamespace ResolveNamespaceForSchema(IXamlSchema schema);
        IXamlNamespace ResolveNamespaceForSchema(string schemaUrl);
    }
}