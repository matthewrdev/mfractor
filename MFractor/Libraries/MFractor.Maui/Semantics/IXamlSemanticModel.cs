using System;
using System.Collections.Generic;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Semantics
{
    public interface IXamlSemanticModel : IDisposable
    {
        ICodeBehindFieldCollection CodeBehindFields { get; }

        ISymbol GetSymbol(XmlNode node);

        ISymbol GetSymbol(XmlAttribute attribute);

        ISymbol GetSymbolForValue(XmlAttribute attribute);

        Expression GetExpression(XmlAttribute attribute);

        XamlExpressionSyntaxNode GetExpressionSyntax(XmlAttribute xmlAttribute);

        ITypeSymbol GetBindingContext(BindingExpression expression, IXamlFeatureContext context);

        ITypeSymbol GetBindingContext(IParsedXamlDocument xamlDocument, Project project, Compilation compilation, IXamlNamespaceCollection namespaces, BindingExpression expression, XmlNode expressionParentNode);

        object GetDataBindingExpressionResult(BindingExpression expression, IXamlFeatureContext context);

        object GetDataBindingExpressionResult(IParsedXamlDocument document, Project project, Compilation compilation, IXamlNamespaceCollection namespaces, BindingExpression expression);
    }
}
