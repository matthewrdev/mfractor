using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    /// <summary>
    /// The <see cref="IBindingContextResolver"/> can resolve the binding context for a provided piece of <see cref="XmlSyntax"/>.
    /// </summary>
    public interface IBindingContextResolver
    {
        /// <summary>
        /// Resolves the binding context for the <paramref name="node"/>.
        /// </summary>
        /// <returns>The binding context.</returns>
        /// <param name="xamlDocument">Xaml document.</param>
        /// <param name="compilation">Compilation.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="node">Node.</param>
        ITypeSymbol ResolveBindingContext(IParsedXamlDocument xamlDocument,
                                          IXamlSemanticModel semanticModel,
                                          IXamlPlatform platform,
                                          Project project,
                                          Compilation compilation,
                                          IXamlNamespaceCollection namespaces,
                                          XmlNode node);

        /// <summary>
        /// Resolves the binding context for the <paramref name="attribute"/>.
        /// </summary>
        /// <returns>The binding context.</returns>
        /// <param name="xamlDocument">Xaml document.</param>
        /// <param name="compilation">Compilation.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="attribute">Attribute.</param>
        ITypeSymbol ResolveBindingContext(IParsedXamlDocument xamlDocument,
                                          IXamlSemanticModel semanticModel,
                                          IXamlPlatform platform,
                                          Project project,
                                          Compilation compilation,
                                          IXamlNamespaceCollection namespaces,
                                          XmlAttribute attribute);

        /// <summary>
        /// Resolves the binding context for the <paramref name="expression"/>.
        /// </summary>
        /// <returns>The binding expression binding context.</returns>
        /// <param name="xamlDocument">Xaml document.</param>
        /// <param name="compilation">Compilation.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="expression">Expression.</param>
        /// <param name="expressionParentNode">Expression parent node.</param>
        ITypeSymbol ResolveBindingContext(IParsedXamlDocument xamlDocument,
                                          IXamlSemanticModel semanticModel,
                                          IXamlPlatform platform,
                                          Project project,
                                          Compilation compilation,
                                          IXamlNamespaceCollection namespaces,
                                          BindingExpression expression,
                                          XmlNode expressionParentNode);
    }
}