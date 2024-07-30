using System;
using MFractor.Maui.Syntax.Parsers;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    public interface IExpressionParserRepository
	{
        TParser ResolveParser<TParser>() where TParser : class, IExpressionParser;

		IExpressionParser ResolveParser (Type type);

		IExpressionParser ResolveParser(XmlAttribute attribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);

		IExpressionParser ResolveParser(string expression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);
	}
}

