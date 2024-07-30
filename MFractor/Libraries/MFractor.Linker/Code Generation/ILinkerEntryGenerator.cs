using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Linker.CodeGeneration
{
    public interface ILinkerEntryGenerator : ICodeGenerator
    {
        bool CanCreateLinkerEntry(ISymbol symbol);

        IReadOnlyList<XmlNode> CreateLinkerEntry(ISymbol symbol);
        IReadOnlyList<XmlNode> CreateLinkerEntry(IAssemblySymbol assembly);
        IReadOnlyList<XmlNode> CreateLinkerEntry(INamedTypeSymbol namedType, LinkerTypeMembersPreserveMode preserveMode);
        IReadOnlyList<XmlNode> CreateLinkerEntry(INamespaceSymbol @namespace);
        IReadOnlyList<XmlNode> CreateLinkerEntry(IFieldSymbol field, LinkerMemberEntryMode linkerMemberEntryMode);
        IReadOnlyList<XmlNode> CreateLinkerEntry(IMethodSymbol method, LinkerMemberEntryMode linkerMemberEntryMode);
        IReadOnlyList<XmlNode> CreateLinkerEntry(IEventSymbol @event);
        IReadOnlyList<XmlNode> CreateLinkerEntry(IPropertySymbol property);
    }
}
