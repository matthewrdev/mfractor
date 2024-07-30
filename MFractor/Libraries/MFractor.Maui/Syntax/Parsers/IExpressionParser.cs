using System.ComponentModel.Composition;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    [InheritedExport]
	public interface IExpressionParser
	{
		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		string Identifier { get; }

		/// <summary>
		/// The name that this parser looks for within a xaml expression.
		/// <para/>
		/// Return a null or empty string if this parser does not evaulate against a name.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; }

		/// <summary>
		/// The priority of the parser when 
		/// </summary>
		/// <value>The parser priority.</value>
		int ParserPriority { get; }

		/// <summary>
		/// If the expression parser can parse the provided expression.
		/// </summary>
		/// <returns>The parse.</returns>
		/// <param name="expression">Expression.</param>
		bool CanParse (string expression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);

        /// <summary>
        /// If the parser can parse the value of the provided attribute into an expression.
        /// </summary>
        /// <returns><c>true</c>, if parse was caned, <c>false</c> otherwise.</returns>
        /// <param name="attribute">Attribute.</param>
		bool CanParse (XmlAttribute attribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);

		Expression Parse (XmlAttribute attribute, Expression parentExpression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);
		Expression Parse (ExpressionComponent expression, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);
		Expression Parse (string expression, TextSpan span, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);
		Expression Parse (string expression, int start, int end, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);
	}
}

