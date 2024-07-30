using MFractor.Maui.Semantics;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    /// <summary>
    /// The markup expression evaluater accepts a XAML expression and evaulates it for it's end result.
    /// </summary>
    public interface IMarkupExpressionEvaluater
    {
        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <returns>The expression.</returns>
        /// <param name="context">Context.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="attribute">Attribute.</param>
        XamlSymbolInfo Evaluate(IXamlFeatureContext context,
                                IXamlNamespaceCollection namespaces,
                                XmlAttribute attribute);

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <returns>The expression.</returns>
        /// <param name="document">Document.</param>
        /// <param name="project">Project.</param>
        /// <param name="compilation">Compilation.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="attribute">Attribute.</param>
        XamlSymbolInfo Evaluate(IParsedXamlDocument document,
                                IXamlSemanticModel xamlSemanticModel,
                                IXamlPlatform platform,
                                Project project,
                                Compilation compilation,
                                IXamlNamespaceCollection namespaces, 
                                XmlAttribute attribute);

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <returns>The expression.</returns>
        /// <param name="xamlDocument">Xaml document.</param>
        /// <param name="project">Project.</param>
        /// <param name="compilation">Compilation.</param>
        /// <param name="namespaces">Namespaces.</param>
        /// <param name="expression">Expression.</param>
        XamlSymbolInfo Evaluate(IParsedXamlDocument xamlDocument,
                                IXamlSemanticModel xamlSemanticModel,
                                IXamlPlatform platform,
                                Project project, 
                                Compilation compilation,
                                IXamlNamespaceCollection namespaces, 
                                Expression expression);

        XamlSymbolInfo EvaluateStaticBindingExpression(Project project,
                                                       IXamlPlatform platform,
                                                       IXamlNamespaceCollection namespaces,
                                                       IXmlnsDefinitionCollection xmlnsDefinitions,
                                                       StaticBindingExpression expression);

        XamlSymbolInfo EvaluateStaticResourceExpression(IParsedXamlDocument document, Project project,  IXamlPlatform platform, Compilation compilation, StaticResourceExpression expression);

        XamlSymbolInfo EvaluateDynamicResourceExpression(IParsedXamlDocument document, Project project, IXamlPlatform platform, Compilation compilation, DynamicResourceExpression expression);

        XamlSymbolInfo EvaluateReferenceExpression(IParsedXamlDocument document,
                                                   Project project,
                                                   IXamlNamespaceCollection namespaces,
                                                   IXmlnsDefinitionCollection xmlnsDefinitions,
                                                   ReferenceExpression expression);

        XamlSymbolInfo EvaluateDataBindingExpression(IParsedXamlDocument document,
                                                     IXamlSemanticModel xamlSemanticModel,
                                                     IXamlPlatform platform,
                                                     Project project,
                                                     Compilation compilation,
                                                     IXamlNamespaceCollection namespaces,
                                                     BindingExpression expression);

        XamlSymbolInfo EvaluateDataBindingExpression(IParsedXamlDocument document,
                                                     IXamlSemanticModel xamlSemanticModel,
                                                     IXamlPlatform platform,
                                                     Project project,
                                                     Compilation compilation,
                                                     IXamlNamespaceCollection namespaces,
                                                     BindingExpression expression,
                                                     ITypeSymbol defaultBindingContext);

        XamlSymbolInfo EvaluateDotNetSymbolExpression(Project project,
                                                      IXamlPlatform platform,
                                                      IXamlNamespaceCollection namespaces,
                                                      IXmlnsDefinitionCollection xmlnsDefinitions,
                                                      DotNetTypeSymbolExpression expression);

        Expression ExtractExpression(XmlAttribute attribute, Project project, IXamlNamespaceCollection xamlNamespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);

        bool CanEvaluate(XmlAttribute attribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);

        ITypeSymbol LocateCodeBehindReferenceBindingContextTypeForExpression(BindingExpression expression, IXamlFeatureContext context);

        ITypeSymbol LocateCodeBehindReferenceBindingContextTypeForExpression(BindingExpression expression,
                                                                             IParsedXamlDocument document,
                                                                             IXamlSemanticModel semanticModel,
                                                                             IXamlPlatform platform,
                                                                             Project project,
                                                                             Compilation compilation,
                                                                             IXamlNamespaceCollection namespaces);
        bool UsesCodeBehindReference(BindingExpression expression);
    }
}
