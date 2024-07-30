using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Code.Documents;
using MFractor.Code;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Workspace;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.CodeGeneration.Xaml
{
    interface IXamlNamespaceImportGenerator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IProjectFile projectFile, IXamlPlatform platform, string xmlNamespaceName, ITypeSymbol symbol, Project project, IXmlFormattingPolicy policy);
        IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IProjectFile projectFile, IXamlPlatform platform, string xmlNamespaceName, string namespaceValue, IXmlFormattingPolicy policy);

        IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IParsedXmlDocument document, IXamlPlatform platform, string xmlNamespaceName, ITypeSymbol symbol, Project project, IXmlFormattingPolicy policy);
        IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IParsedXmlDocument document, IXamlPlatform platform, string xmlNamespaceName, string namespaceValue, IXmlFormattingPolicy policy);

        IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(XmlSyntaxTree syntaxTree, string filePath, string xmlNamespaceName, ITypeSymbol symbol, Project project, IXmlFormattingPolicy policy);
        IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(XmlSyntaxTree syntaxTree, IXamlPlatform platform, string filePath, string xmlNamespaceName, string namespaceValue, IXmlFormattingPolicy policy);

        XmlAttribute GenerateXmlnsImportAttibute(string prefix, string namespaceValue);
        XmlAttribute GenerateXmlnsImportAttibute(string prefix, ITypeSymbol symbol, bool includeAssembly);
        XmlAttribute GenerateXmlnsImportAttibute(string prefix, string namespaceName, string assemblyName, bool includeAssembly);
        XmlAttribute GenerateXmlnsImportAttibute(string prefix, INamespaceSymbol namespaceSymbol, bool includeAssembly);

        string GenerateXmlnsImportAttibuteValue(INamespaceSymbol namespaceSymbol, bool includeAssembly);
        string GenerateXmlnsImportAttibuteValue(string namespaceName, string assemblyName, bool includeAssembly);
        string GenerateXmlnsImportStatement(string prefix, ITypeSymbol symbol, bool includeAssembly);
        string GenerateXmlnsImportStatement(string prefix, string namespaceName, string assemblyName, bool includeAssembly);

        XmlAttribute GenerateXmlnsUriImportAttibute(string xmlns, string name, string uriContent);
    }
}