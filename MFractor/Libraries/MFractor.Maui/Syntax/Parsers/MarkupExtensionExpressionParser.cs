using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Xmlns;
using MFractor.IOC;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Syntax.Parsers
{
    abstract class MarkupExtensionExpressionParser<TExpression> : ExpressionParser where TExpression : MarkupExtensionExpression
    {
        protected abstract IXamlNamespace GetXamlNamespace(IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);

        readonly Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver = new Lazy<IXmlnsNamespaceSymbolResolver>(Resolver.Resolve<IXmlnsNamespaceSymbolResolver>);
        public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver => xmlnsNamespaceSymbolResolver.Value;

        public override bool CanParse(string expression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            try
            {
                var xmlns = GetXamlNamespace(namespaces, xmlnsDefinitions, platform);
                if (xmlns is null)
                {
                    return false;
                }

                var match = Name;
                if (!string.IsNullOrEmpty(xmlns.Prefix))
                {
                    match = xmlns.Prefix + ":" + this.Name;
                }

                if (ExpressionParserHelper.BeginsWithKeyword(expression, match))
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }


        public override Expression Parse(string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            TextSpan fullNameRegion;
            if (!ExpressionParserHelper.ResolveMarkupExtensionExpressionNameAndRegions(expression,
                                                                                       expressionStart,
                                                                                       out fullNameRegion,
                                                                                       out var fullName,
                                                                                       out _,
                                                                                       out _,
                                                                                       out _,
                                                                                       out _))
            {
                return null;
            }

            var rootExpression = Create(fullName, fullNameRegion, TextSpan.FromBounds(expressionStart, expressionEnd), parentExpression, parentAttribute);

            var components = ExpressionParserHelper.ParseExpressionComponents(expression, expressionStart);

            var expressions = new List<Expression>();
            foreach (var c in components)
            {

                var parsedExpression = ParseExpressionComponent(c, rootExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);
                if (parsedExpression != null)
                {
                    expressions.Add(parsedExpression);
                }
            }

            if (expression.Any())
            {
                rootExpression.Children.AddRange(expressions);
            }

            return rootExpression;
        }

        protected abstract TExpression Create(string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute);

        protected virtual Expression ParseExpressionComponent(ExpressionComponent c, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            Expression result = null;

            IExpressionParser parser = null;
            var component = c;

            if (CanParse(c.Content, project, namespaces, xmlnsDefinitions, platform))
            {
                // Resolve the value expression
                if (ExpressionParserHelper.ExtractAssignmentExpressionComponents(c.Content, c.Span.Start, out _, out var value))
                {
                    if (ExpressionParserHelper.IsPropertyAssignmentExpression(value.Content))
                    {
                        parser = ParserProvider.ResolveParser(value.Content, project, namespaces, xmlnsDefinitions, platform);
                        component = value;
                    }
                    else
                    {
                        result = CreateValueExpression(value, parentExpression, parentAttribute);
                    }
                }

            }
            else
            {
                parser = ParserProvider.ResolveParser(c.Content, project, namespaces, xmlnsDefinitions, platform);
            }

            if (result == null && parser != null)
            {
                result = parser.Parse(component, parentExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);
            }

            return result;
        }

        protected virtual ValueExpression CreateValueExpression(ExpressionComponent value, Expression parentExpression, XmlAttribute parentAttribute)
        {
            return new LiteralValueExpression(value, parentExpression, parentAttribute);
        }
    }
}

