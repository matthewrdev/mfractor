using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Symbols
{
    public interface IXamlSymbolResolver
	{
        XamlSymbolInfo Resolve(IParsedXamlDocument document,
                               IXamlSemanticModel semanticModel,
                               IXamlPlatform platform,
                               Project project,
                               Compilation compilation,
                               IXamlNamespaceCollection namespaces,
                               int position);

        XamlSymbolInfo Resolve(IParsedXamlDocument document,
                               IXamlSemanticModel semanticModel,
                               IXamlPlatform platform,
                               Project project,
                               Compilation compilation, 
                               IXamlNamespaceCollection namespaces,
                               XmlSyntax element, 
                               int? position = null);

        XamlSymbolInfo ResolveXamlNode(IParsedXamlDocument document,
                                       Project project,
                                       Compilation compilation,
                                       IXamlPlatform platform,
                                       IXamlNamespaceCollection namespaces,
                                       XmlNode element);

        ISymbol ResolveSymbol(IParsedXamlDocument document,
                              Project project,
                              Compilation compilation,
                                       IXamlPlatform platform,
                              IXamlNamespaceCollection namespaces,
                              string elementFullName,
                              bool hasTypeArgument = false);

        XamlSymbolInfo ResolveAttribute(IParsedXamlDocument document,
                                        Project project,
                                        Compilation compilation,
                                       IXamlPlatform platform,
                                        IXamlNamespaceCollection namespaces,
                                        XmlAttribute attribute);

        XamlSymbolInfo ResolveAttribute(IParsedXamlDocument document,
                                        Project project,
                                        Compilation compilation,
                                       IXamlPlatform platform,
                                        IXamlNamespaceCollection namespaces, 
                                        XamlSymbolInfo elementResult, 
                                        XmlAttribute attribute);

        ISymbol ResolveSymbol(IParsedXamlDocument document,
                              Project project,
                              Compilation compilation,
                                       IXamlPlatform platform,
                              IXamlNamespaceCollection namespaces,
                              ITypeSymbol target, 
                              string symbolName);

        XamlSymbolInfo ResolveAttributeValue(IParsedXamlDocument document,
                                             IXamlSemanticModel semanticModel,
                                             IXamlPlatform platform,
                                             Project project,
                                             Compilation compilation, 
                                             IXamlNamespaceCollection namespaces,
                                             XmlAttribute attribute,
                                             ISymbol attributeSymbol);

        INamedTypeSymbol ResolveMarkupExtension(Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, NameSyntax nameSyntax);

        ISymbol ResolveCodeBehindSymbol(INamedTypeSymbol codeBehindClass, string name);

        ISymbol ResolveReferencedAttributeValueSymbolByName(INamedTypeSymbol codeBehindClass, ISymbol referencedSymbol, string nameToResolve);
    }
}
