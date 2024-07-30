using System;
using System.Collections.Generic;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Semantics
{
    class XamlSemanticModel : IXamlSemanticModel
    {
        readonly IXamlSymbolResolver xamlSymbolresolver;
        readonly IMarkupExpressionEvaluater expressionEvaluater;
        readonly IBindingContextResolver bindingContextResolver;

        public XamlSemanticModel(IParsedXamlDocument document,
                                 Project project,
                                 Compilation compilation,
                                 IXamlPlatform xamlPlatform,
                                 IXamlNamespaceCollection namespaces,
                                 IXamlSymbolResolver xamlSymbolresolver,
                                 IMarkupExpressionEvaluater expressionEvaluater,
                                 IBindingContextResolver bindingContextResolver)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            XamlPlatform = xamlPlatform ?? throw new ArgumentNullException(nameof(xamlPlatform));
            Namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
            this.xamlSymbolresolver = xamlSymbolresolver;
            this.expressionEvaluater = expressionEvaluater;
            this.bindingContextResolver = bindingContextResolver;
            CodeBehindFields = new CodeBehindFieldCollection(Document.XamlSyntaxTree);
        }

        public IParsedXamlDocument Document { get; }
        public Project Project { get; }
        public Compilation Compilation { get; }
        public IXamlPlatform XamlPlatform { get; }
        public IXamlNamespaceCollection Namespaces { get; }

        public ICodeBehindFieldCollection CodeBehindFields { get; }

        readonly Dictionary<XmlSyntax, XamlSymbolInfo> symbolCache = new Dictionary<XmlSyntax, XamlSymbolInfo>();
        readonly Dictionary<XmlSyntax, XamlSymbolInfo> attributeValueCache = new Dictionary<XmlSyntax, XamlSymbolInfo>();
        readonly Dictionary<XmlSyntax, Expression> expressionCache = new Dictionary<XmlSyntax, Expression>();
        readonly Dictionary<XmlSyntax, XamlExpressionSyntaxNode> expressionSyntaxCache = new Dictionary<XmlSyntax, XamlExpressionSyntaxNode>();
        readonly Dictionary<Expression, ITypeSymbol> bindingExpressionContextCache = new Dictionary<Expression, ITypeSymbol>();
        readonly Dictionary<Expression, object> bindingExpressionResultCache = new Dictionary<Expression, object>();

        public void Dispose()
        {
            symbolCache.Clear();
            expressionCache.Clear();
            expressionSyntaxCache.Clear();
            attributeValueCache.Clear();
            bindingExpressionContextCache.Clear();
            bindingExpressionResultCache.Clear();
        }

        public XamlExpressionSyntaxNode GetExpressionSyntax(XmlAttribute attribute)
        {
            if (attribute is null)
            {
                return default;
            }

            if (expressionSyntaxCache.TryGetValue(attribute, out var @value))
            {
                return @value;
            }

            if (!attribute.HasValue)
            {
                expressionSyntaxCache[attribute] = null;
                return null;
            }

            XamlExpressionSyntaxNode syntaxNode = default;

            var parser = new XamlExpressionParser(attribute.Value.Value, attribute.Value.Span);

            if (ExpressionParserHelper.IsExpression(attribute.Value?.Value))
            {
                syntaxNode = parser.Parse();
            }
            else
            {
                var symbol = SymbolHelper.ResolveMemberReturnType(GetSymbol(attribute));
                if (SymbolHelper.DerivesFrom(symbol, "System.Type") && attribute.HasValue)
                {
                    syntaxNode = parser.Parse();
                    // syntaxNode = new DotNetTypeSymbolExpression(attribute.Value.Value, attribute.Value.Span, null, attribute);
                }
            }

            expressionSyntaxCache[attribute] = syntaxNode;

            return syntaxNode;

        }

        public Expression GetExpression(XmlAttribute attribute)
        {
            if (attribute is null)
            {
                return default;
            }

            if (expressionCache.TryGetValue(attribute, out var @value))
            {
                return @value;
            }

            Expression expression = default;

            if (expressionEvaluater.CanEvaluate(attribute, Project, Namespaces, Document.XmlnsDefinitions, XamlPlatform))
            {
                expression = expressionEvaluater.ExtractExpression(attribute, Project, Namespaces, Document.XmlnsDefinitions, XamlPlatform);
            }
            else
            {
                var symbol = SymbolHelper.ResolveMemberReturnType(GetSymbol(attribute));
                if (SymbolHelper.DerivesFrom(symbol, "System.Type") && attribute.HasValue)
                {
                    expression = new DotNetTypeSymbolExpression(attribute.Value.Value, attribute.Value.Span, null, attribute);
                }
            }


            expressionCache[attribute] = expression;

            return expression;
        }

        public ISymbol GetSymbol(XmlNode node)
        {
            if (node is null)
            {
                return default;
            }

            if (symbolCache.TryGetValue(node, out var @value))
            {
                return @value.GetSymbol<ISymbol>();
            }

            var result = xamlSymbolresolver.ResolveXamlNode(Document, Project, Compilation, XamlPlatform, Namespaces, node);

            if (result != null)
            {
                symbolCache[node] = result;
            }
            else
            {
                return default;
            }

            return result.GetSymbol<ISymbol>();
        }

        public ISymbol GetSymbol(XmlAttribute attribute)
        {
            if (attribute is null)
            {
                return default;
            }

            if (symbolCache.TryGetValue(attribute, out var @value))
            {
                return @value.GetSymbol<ISymbol>();
            }

            var result = xamlSymbolresolver.ResolveAttribute(Document, Project, Compilation, XamlPlatform, Namespaces, attribute);

            if (result != null)
            {
                symbolCache[attribute] = result;
            }
            else
            {
                return default;
            }

            return result.GetSymbol<ISymbol>();
        }

        public ISymbol GetSymbolForValue(XmlAttribute attribute)
        {
            if (attribute is null)
            {
                return default;
            }

            if (attributeValueCache.TryGetValue(attribute, out var @value))
            {
                return @value.GetSymbol<ISymbol>();
            }

            var attributeSymbol = GetSymbol(attribute);

            var result = xamlSymbolresolver.ResolveAttributeValue(this.Document, this, XamlPlatform, Project, Compilation, Namespaces, attribute, attributeSymbol);

            if (result != null)
            {
                attributeValueCache[attribute] = result;
            }
            else
            {
                return default;
            }

            return result.GetSymbol<ISymbol>();
        }

        public ITypeSymbol GetBindingContext(BindingExpression expression, IXamlFeatureContext context)
        {
            if (context == null || expression == null)
            {
                return default;
            }

            return GetBindingContext(context.XamlDocument, context.Project, context.Compilation, context.Namespaces, expression, expression.ParentAttribute.Parent);
        }

        public ITypeSymbol GetBindingContext(IParsedXamlDocument xamlDocument,
                                             Project project,
                                             Compilation compilation,
                                             IXamlNamespaceCollection namespaces,
                                             BindingExpression expression,
                                             XmlNode expressionParentNode)
        {
            if (expression == null)
            {
                return default;
            }

            if (bindingExpressionContextCache.ContainsKey(expression))
            {
                return bindingExpressionContextCache[expression];
            }

            var bindingContext = bindingContextResolver.ResolveBindingContext(xamlDocument, this, XamlPlatform, project, compilation, namespaces, expression, expressionParentNode);

            bindingExpressionContextCache[expression] = bindingContext;

            return bindingContext;
        }

        public object GetDataBindingExpressionResult(IParsedXamlDocument document, Project project, Compilation compilation, IXamlNamespaceCollection namespaces, BindingExpression expression)
        {
            if (expression == null)
            {
                return default;
            }

            if (bindingExpressionResultCache.ContainsKey(expression))
            {
                return bindingExpressionResultCache[expression];
            }

            var bindingContext = GetBindingContext(document, project, compilation, namespaces, expression, expression.ParentAttribute.Parent);

            var result = expressionEvaluater.EvaluateDataBindingExpression(document, this, XamlPlatform, project, compilation, namespaces, expression, bindingContext);

            bindingExpressionResultCache[expression] = result?.Symbol;

            return result?.Symbol;
        }

        public object GetDataBindingExpressionResult(BindingExpression expression, IXamlFeatureContext context)
        {
            if (context == null || expression == null)
            {
                return default;
            }

            return GetDataBindingExpressionResult(context.XamlDocument, context.Project, context.Compilation, context.Namespaces, expression);
        }
    }
}