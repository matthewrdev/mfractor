using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Semantics;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMarkupExpressionEvaluater))]
    class MarkupExpressionEvaluater : IMarkupExpressionEvaluater
    {
        readonly Lazy<IExpressionParserRepository> expressionParserRepository;
        IExpressionParserRepository ExpressionParserRepository => expressionParserRepository.Value;

        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        readonly Lazy<IStaticResourceResolver> staticResourceResolver;
        IStaticResourceResolver StaticResourceResolver => staticResourceResolver.Value;

        readonly Lazy<IDynamicResourceResolver> dynamicResourceResolver;
        IDynamicResourceResolver DynamicResourceResolver => dynamicResourceResolver.Value;

        readonly Lazy<Symbols.IXamlTypeResolver> xamlTypeResolver;
        Symbols.IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        [ImportingConstructor]
        public MarkupExpressionEvaluater(Lazy<IExpressionParserRepository> expressionParserRepository,
                                         Lazy<IBindingContextResolver> bindingContextResolver,
                                         Lazy<IStaticResourceResolver> staticResourceResolver,
                                         Lazy<IDynamicResourceResolver> dynamicResourceResolver,
                                         Lazy<IXamlTypeResolver> xamlTypeResolver)
        {
            this.expressionParserRepository = expressionParserRepository;
            this.bindingContextResolver = bindingContextResolver;
            this.staticResourceResolver = staticResourceResolver;
            this.dynamicResourceResolver = dynamicResourceResolver;
            this.xamlTypeResolver = xamlTypeResolver;
        }

        public XamlSymbolInfo Evaluate(IXamlFeatureContext context,
                                       IXamlNamespaceCollection namespaces,
                                       XmlAttribute attribute)
        {
            return Evaluate(context.XamlDocument,
                            context.XamlSemanticModel,
                            context.Platform,
                            context.Project,
                            context.Compilation,
                            namespaces, attribute);
        }

        public XamlSymbolInfo Evaluate(IParsedXamlDocument xamlDocument,
                                       IXamlSemanticModel xamlSemanticModel,
                                       IXamlPlatform platform,
                                       Project project,
                                       Compilation compilation,
                                       IXamlNamespaceCollection namespaces,
                                       XmlAttribute attribute)
        {
            var parsedExpression = ExtractExpression(attribute, project, namespaces, xamlDocument.XmlnsDefinitions, platform);

            return Evaluate(xamlDocument, xamlSemanticModel, platform, project, compilation, namespaces, parsedExpression);
        }

        public XamlSymbolInfo Evaluate(IParsedXamlDocument document,
                                       IXamlSemanticModel semanticModel,
                                       IXamlPlatform platform,
                                       Project project,
                                       Compilation compilation,
                                       IXamlNamespaceCollection namespaces,
                                       Expression expression)
        {
            XamlSymbolInfo result = null;

            if (expression is StaticBindingExpression staticBinding)
            {
                result = EvaluateStaticBindingExpression(project, platform, namespaces, document.XmlnsDefinitions, staticBinding);
            }
            else if (expression is BindingExpression binding)
            {
                var bindingContext = BindingContextResolver.ResolveBindingContext(document, semanticModel, platform, project, compilation, namespaces, binding, expression.ParentAttribute.Parent);

                result = EvaluateDataBindingExpression(document, semanticModel, platform, project, compilation, namespaces, binding, bindingContext);
            }
            else if (expression is ReferenceExpression referenceExpression)
            {
                result = EvaluateReferenceExpression(document, project, namespaces, document.XmlnsDefinitions, referenceExpression);
            }
            else if (expression is StaticResourceExpression staticResourceExpression)
            {
                result = EvaluateStaticResourceExpression(document, project, platform, compilation, staticResourceExpression);
            }
            else if (expression is DynamicResourceExpression dynamicResourceExpression)
            {
                result = EvaluateDynamicResourceExpression(document, project, platform, compilation, dynamicResourceExpression);
            }
            else
            {
                if (expression is DefaultMarkupExtensionExpression defaultMarkupExtension)
                {

                }
            }


            if (result != null && expression != null)
            {
                result.Expression = expression;
                result.Syntax = expression.ParentAttribute;
            }
            else if (expression != null)
            {
                result = new XamlSymbolInfo()
                {
                    Symbol = null,
                    Syntax = expression?.ParentAttribute,
                    Expression = expression,
                    Span = expression?.Span ?? default,
                };
            }

            return result;
        }

        public XamlSymbolInfo EvaluateStaticBindingExpression(Project project,
                                                              IXamlPlatform platform,
                                                              IXamlNamespaceCollection namespaces,
                                                              IXmlnsDefinitionCollection xmlnsDefinitions,
                                                              StaticBindingExpression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.SymbolExpression == null)
            {
                return null;
            }

            return EvaluateDotNetSymbolExpression(project, platform, namespaces, xmlnsDefinitions, expression.SymbolExpression);
        }

        public XamlSymbolInfo EvaluateDataBindingExpression(IParsedXamlDocument document,
                                                            IXamlSemanticModel semanticModel,
                                                            IXamlPlatform platform,
                                                            Project project,
                                                            Compilation compilation,
                                                            IXamlNamespaceCollection namespaces,
                                                            BindingExpression expression,
                                                            ITypeSymbol defaultBindingContext)
        {
            var expressionBindingContext = defaultBindingContext;

            if (expression.HasReferencedSymbol == false)
            {
                return null;
            }

            var valueSpan = expression.ReferencedSymbolSpan;
            var value = expression.ReferencedSymbolValue;

            ISymbol symbol = null;

            if (value.Trim() == ".")
            {
                symbol = defaultBindingContext;
            }
            else if (value.StartsWith($"{platform.BindingContextProperty}.")) // Is this expression inspecting into the runtime binding context?
            {
                if (UsesCodeBehindReference(expression))
                {
                    value = value.Remove(0, $"{platform.BindingContextProperty}.".Length);
                    expressionBindingContext = LocateCodeBehindReferenceBindingContextTypeForExpression(expression,
                                                                                                        document,
                                                                                                        semanticModel,
                                                                                                        platform,
                                                                                                        project,
                                                                                                        compilation,
                                                                                                        namespaces);

                    symbol = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(expressionBindingContext, value);
                }
            }
            else if (value.Contains('.'))
            {
                var components = value.Split('.');

                foreach (var c in components)
                {
                    symbol = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(expressionBindingContext, c);
                    if (symbol == null)
                    {
                        symbol = SymbolHelper.FindMemberSymbolByName<IFieldSymbol>(expressionBindingContext, c);
                    }

                    if (symbol == null)
                    {
                        break;
                    }

                    if (symbol is IPropertySymbol)
                    {
                        expressionBindingContext = (symbol as IPropertySymbol).Type;
                    }
                    else
                    {
                        expressionBindingContext = (symbol as IFieldSymbol).Type;
                    }
                }
            }
            else
            {
                symbol = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(expressionBindingContext, value);
                if (symbol == null)
                {
                    symbol = SymbolHelper.FindMemberSymbolByName<IFieldSymbol>(expressionBindingContext, value);
                }
            }

            var result = new XamlSymbolInfo()
            {
                Symbol = symbol,
                BindingContext = expressionBindingContext,
                Syntax = expression.ParentAttribute,
                Expression = expression,
                Span = valueSpan,
            };

            return result;
        }

        public bool UsesCodeBehindReference(BindingExpression expression)
        {
            return expression?.Source?.Reference != null;
        }

        public ITypeSymbol LocateCodeBehindReferenceBindingContextTypeForExpression(BindingExpression expression,
                                                                           IXamlFeatureContext context)
        {
            return LocateCodeBehindReferenceBindingContextTypeForExpression(expression, context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces);
        }

        public ITypeSymbol LocateCodeBehindReferenceBindingContextTypeForExpression(BindingExpression expression,
                                                                                    IParsedXamlDocument document,
                                                                                    IXamlSemanticModel semanticModel,
                                                                                    IXamlPlatform platform,
                                                                                    Project project,
                                                                                    Compilation compilation,
                                                                                    IXamlNamespaceCollection namespaces)
        {
            var referenceExpression = expression?.Source?.Reference;

            var element = semanticModel.CodeBehindFields.GetCodeBehindField(referenceExpression?.ReferencedXNameValue);

            if (element is null)
            {
                return null;
            }

            return BindingContextResolver.ResolveBindingContext(document, semanticModel, platform, project, compilation, namespaces, element.Node);
        }

        public XamlSymbolInfo EvaluateDataBindingExpression(IParsedXamlDocument document,
                                                            IXamlSemanticModel semanticModel,
                                                            IXamlPlatform platform,
                                                            Project project,
                                                            Compilation compilation,
                                                            IXamlNamespaceCollection namespaces,
                                                            BindingExpression expression)
        {
            var symbol = BindingContextResolver.ResolveBindingContext(document, semanticModel, platform, project, compilation, namespaces, expression, expression.ParentAttribute.Parent);

            if (symbol == null)
            {
                return null;
            }

            return EvaluateDataBindingExpression(document, semanticModel, platform, project, compilation, namespaces, expression, symbol);
        }

        public Expression ExtractExpression(XmlAttribute attribute, Project  project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            if (!attribute.HasValue)
            {
                return null;
            }

            var parser = ExpressionParserRepository.ResolveParser(attribute.Value.Value, project, namespaces, xmlnsDefinitions, platform)
                                                  ?? ExpressionParserRepository.ResolveParser(attribute, project, namespaces, xmlnsDefinitions, platform);

            if (parser == null)
            {
                return null;
            }

            return parser.Parse(attribute, null, project, namespaces, xmlnsDefinitions, platform);
        }


        INamedTypeSymbol GetNamedTypeSymbol(XmlNode node, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (node == null || project == null || namespaces == null || xmlnsDefinitions  is null)
            {
                return null;
            }

            var xmlns = namespaces.ResolveNamespace(node);

            if (xmlns == null)
            {
                return null;
            }

            return XamlTypeResolver.ResolveType(node.Name.LocalName, xmlns, project, xmlnsDefinitions);
        }

        public XamlSymbolInfo EvaluateDotNetSymbolExpression(Project project,
                                                             IXamlPlatform platform,
                                                             IXamlNamespaceCollection namespaces,
                                                             IXmlnsDefinitionCollection xmlnsDefinitions,
                                                             DotNetTypeSymbolExpression expression)
        {
            var result = new XamlSymbolInfo();

            if (expression == null
                || expression.HasValue == false
                || expression.HasSymbol == false)
            {
                return null;
            }

            var xamlNamespace = namespaces.ResolveNamespace(expression.Namespace);

            if (xamlNamespace == null)
            {
                return null;
            }

            IPropertySymbol outerProperty = null;
            if (expression.ParentExpression == null && expression.ParentAttribute != null)
            {
                var parentSymbol = GetNamedTypeSymbol(expression.ParentAttribute.Parent, project, namespaces, xmlnsDefinitions);

                if (parentSymbol != null)
                {
                    outerProperty = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(parentSymbol, expression.ParentAttribute.Name.LocalName);
                }
            }

            var components = expression.Symbol.Split('.').ToList();
            var className = components[0];

            var dotNetExpression = expression;

            if (dotNetExpression == null || dotNetExpression.IsMarkupExtension)
            {
                className += "Extension";
            }

            var symbols = components.GetRange(1, components.Count - 1);

            ITypeSymbol typeSymbol = XamlTypeResolver.ResolveType(className, xamlNamespace, project, xmlnsDefinitions);
            if (typeSymbol == null)
            {
                return null;
            }

            var isBindable = outerProperty != null && SymbolHelper.DerivesFrom(outerProperty.Type, platform.BindableProperty.MetaType);

            ISymbol resolvedSymbol = typeSymbol;
            if (symbols.Count == 1 && isBindable)
            {
                var bindableName = symbols[0] + "Property";
                resolvedSymbol = SymbolHelper.FindMemberSymbolByName<IFieldSymbol>(typeSymbol, bindableName);
            }
            else
            {
                for (var i = 0; i < symbols.Count; ++i)
                {
                    var prop = symbols[i];

                    resolvedSymbol = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(typeSymbol, prop);
                    if (resolvedSymbol == null)
                    {
                        resolvedSymbol = SymbolHelper.FindMemberSymbolByName<IFieldSymbol>(typeSymbol, prop);
                    }

                    if (resolvedSymbol == null)
                    {
                        break;
                    }

                    if (resolvedSymbol is IPropertySymbol)
                    {
                        typeSymbol = (resolvedSymbol as IPropertySymbol).Type;
                    }
                    else
                    {
                        typeSymbol = (resolvedSymbol as IFieldSymbol).Type;
                    }
                }
            }

            result = new XamlSymbolInfo()
            {
                Symbol = resolvedSymbol,
                Syntax = expression.ParentAttribute,
                Expression = expression,
                Span = expression.Span,
            };

            return result;
        }

        public bool CanEvaluate(XmlAttribute attribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            if (!attribute.HasValue)
            {
                return false;
            }

            if (!ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return false;
            }

            var parser = ExpressionParserRepository.ResolveParser(attribute, project, namespaces, xmlnsDefinitions, platform);

            return parser != null;
        }

        public XamlSymbolInfo EvaluateStaticResourceExpression(IParsedXamlDocument document,
                                                               Project project,
                                                               IXamlPlatform platform,
                                                               Compilation compilation,
                                                               StaticResourceExpression expression)
        {
            if (expression.Value == null
                || expression.Value.HasValue == false)
            {
                return null;
            }

            var resources = StaticResourceResolver.FindNamedStaticResources(project, platform, document.FilePath, expression.Value.Value);

            if (resources == null || !resources.Any())
            {
                return null;
            }

            var resource = resources.First();

            var symbol = compilation.GetTypeByMetadataName(resource.ReturnType);

            return new XamlSymbolInfo()
            {
                Symbol = symbol,
                Span = expression.Span,
                AdditionalData = new StaticResourceResult(resource, resources.GetProjectFor(resource)),
                SymbolKind = XamlSymbolKind.StaticResource,
                Expression = expression,
            };
        }

        public XamlSymbolInfo EvaluateDynamicResourceExpression(IParsedXamlDocument document,
                                                                Project project,
                                                                IXamlPlatform platform,
                                                                Compilation compilation,
                                                                DynamicResourceExpression expression)
        {
            if (expression.Value == null
                || expression.Value.HasValue == false)
            {
                return null;
            }

            var resources = DynamicResourceResolver.FindAvailableNamedDynamicResources(project, platform, document.FilePath, expression.Value.Value);

            if (resources == null || !resources.Any())
            {
                return null;
            }

            var resource = resources.First();

            var symbol = compilation.GetTypeByMetadataName(resource.Definition.ReturnType);

            return new XamlSymbolInfo()
            {
                Symbol = symbol,
                Span = expression.Span,
                AdditionalData = resource,
                SymbolKind = XamlSymbolKind.DynamicResource,
                Expression = expression,
            };
        }

        XmlNode FindCodeBehindField(string name, XmlNode node)
        {
            if (node.HasAttribute("x:Name") && node.GetAttributeByName("x:Name").Value?.Value == name)
            {
                return node;
            }

            if (!node.HasChildren)
            {
                return null;
            }

            foreach (var child in node.Children)
            {
                var result = FindCodeBehindField(name, child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public XamlSymbolInfo EvaluateReferenceExpression(IParsedXamlDocument document,
                                                          Project project,
                                                          IXamlNamespaceCollection namespaces,
                                                          IXmlnsDefinitionCollection xmlnsDefinitions,
                                                          ReferenceExpression expression)
        {
            var valueExpression = expression.ReferencedXNameValue;

            var node = FindCodeBehindField(valueExpression, document.XamlSyntaxRoot);

            if (node == null)
            {
                return null;
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            if (node.HasAttribute("x:Class"))
            {
                var xClass = node.GetAttributeByName("x:Class");

                var symbol = compilation.GetTypeByMetadataName(xClass.Value.Value);

                if (symbol == null)
                {
                    return null;
                }

                return new XamlSymbolInfo()
                {
                    Symbol = symbol,
                    SymbolKind = XamlSymbolKind.Symbol,
                    Expression = expression,
                    Span = expression.Span,
                };
            }
            else
            {

                var value = node.Name.FullName;
                var namespaceName = "";
                var typeName = value;
                if (value.Contains(":"))
                {
                    var components = value.Split(':');
                    if (components.Length < 2)
                    {
                        return null;
                    }

                    namespaceName = components[0];
                    typeName = components[1];
                }

                var xmlns = namespaces.ResolveNamespace(namespaceName);

                var symbol = XamlTypeResolver.ResolveType(typeName, xmlns, project, xmlnsDefinitions);

                if (symbol == null)
                {
                    return null;
                }

                return new XamlSymbolInfo()
                {
                    Symbol = symbol,
                    SymbolKind = XamlSymbolKind.Symbol,
                    Expression = expression,
                    Span = expression.Span,
                };
            }
        }
    }
}

