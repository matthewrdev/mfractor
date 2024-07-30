using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Semantics;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IBindingContextResolver))]
    class BindingContextResolver : IBindingContextResolver
    {
        readonly Lazy<IExpressionParserRepository> parserRepository;
        public IExpressionParserRepository ParserRepository => parserRepository.Value;

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        readonly Lazy<IXamlSymbolResolver> symbolResolver;
        public IXamlSymbolResolver SymbolResolver => symbolResolver.Value;

        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        [ImportingConstructor]
        public BindingContextResolver(Lazy<IExpressionParserRepository> parserRepository,
                                      Lazy<IMarkupExpressionEvaluater> expressionEvaluater,
                                      Lazy<IXamlSymbolResolver> symbolResolver,
                                      Lazy<IXamlTypeResolver> xamlTypeResolver)
        {
            this.parserRepository = parserRepository;
            this.expressionEvaluater = expressionEvaluater;
            this.symbolResolver = symbolResolver;
            this.xamlTypeResolver = xamlTypeResolver;
        }


        XmlNode FindCodeBehindField(string name, XmlNode node)
        {
            if (node.HasAttribute("x:Name")
                && node.GetAttributeByName("x:Name").Value?.Value == name)
            {
                return node;
            }

            if (!node.HasChildren)
            {
                return default;
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

        protected ITypeSymbol ResolveReferenceSymbol(IParsedXamlDocument xamlDocument,
                                                     Project project,
                                                     Compilation compilation,
                                                     IXamlNamespaceCollection namespaces,
                                                     ReferenceExpression expression)
        {
            var xNamedNode = FindCodeBehindField(expression.ReferencedXNameValue, xamlDocument.XamlSyntaxRoot);

            if (xNamedNode == null)
            {
                return null;
            }

            if (xNamedNode.HasAttribute("x:Class"))
            {
                var xClass = xNamedNode.GetAttributeByName("x:Class");

                return compilation.GetTypeByMetadataName(xClass?.Value?.Value);
            }

            return GetNamedTypeSymbol(xNamedNode, project, namespaces, xamlDocument.XmlnsDefinitions);
        }

        public ITypeSymbol ResolveAttributeValueBindingContext(IParsedXamlDocument xamlDocument,
                                                               IXamlSemanticModel semanticModel,
                                                               IXamlPlatform platform,
                                                               Project project,
                                                               Compilation compilation,
                                                               IXamlNamespaceCollection namespaces,
                                                               XmlAttribute attribute)
        {
            if (!attribute.HasValue)
            {
                return null;
            }

            var parser = ParserRepository.ResolveParser(attribute.Value.Value, project, namespaces, xamlDocument.XmlnsDefinitions, platform);
            var expression = parser.Parse(attribute, null, project, namespaces, xamlDocument.XmlnsDefinitions, platform);

            if (!(expression is DefaultMarkupExtensionExpression))
            {
                if (expression is ReferenceExpression referenceExpression)
                {
                    var result = ResolveReferenceSymbol(xamlDocument, project, compilation, namespaces, referenceExpression);

                    return result;
                }
                else if (expression is StaticBindingExpression)
                {
                    var staticBinding = expression as StaticBindingExpression;

                    var result = ExpressionEvaluater.EvaluateStaticBindingExpression(project, platform, namespaces, xamlDocument.XmlnsDefinitions, staticBinding);

                    if (result == null || result.Symbol == null)
                    {
                        return null;
                    }

                    var symbol = result.Symbol as ISymbol;

                    ITypeSymbol typeSymbol = default;

                    if (symbol is ITypeSymbol type)
                    {
                        typeSymbol = type;
                    }
                    else if (symbol is IPropertySymbol property)
                    {
                        typeSymbol = property.Type;
                    }
                    else if (symbol is IFieldSymbol field)
                    {
                        typeSymbol = field.Type;
                    }

                    return typeSymbol;
                }
                else if (expression is BindingExpression)
                {
                    var bindingExpression = expression as BindingExpression;

                    var result = ExpressionEvaluater.EvaluateDataBindingExpression(xamlDocument, semanticModel, platform, project, compilation, namespaces, bindingExpression);

                    if (result == null || result.Symbol == null)
                    {
                        if (TryResolveSpecialExpressionBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, bindingExpression, attribute.Parent, out var bindingContext))
                        {
                            return bindingContext;
                        }

                        return null;
                    }

                    var symbol = result.Symbol as ISymbol;

                    ITypeSymbol typeSymbol = null;

                    if (symbol is ITypeSymbol type)
                    {
                        typeSymbol = type;
                    }
                    else if (symbol is IPropertySymbol property)
                    {
                        typeSymbol = property.Type;
                    }
                    else if (symbol is IFieldSymbol field)
                    {
                        typeSymbol = field.Type;
                    }

                    return typeSymbol;
                }
                else if (expression is StaticResourceExpression resourceExpression)
                {
                    var result = ExpressionEvaluater.EvaluateStaticResourceExpression(xamlDocument, project, platform, compilation, resourceExpression);

                    return result.GetSymbol<INamedTypeSymbol>();
                }
            }

            return null;
        }

        public ITypeSymbol ResolveBindingContext(IParsedXamlDocument xamlDocument,
                                                 IXamlSemanticModel semanticModel,
                                                 IXamlPlatform platform,
                                                 Project project,
                                                 Compilation compilation,
                                                 IXamlNamespaceCollection namespaces,
                                                 XmlAttribute attribute)
        {
            if (ExpressionParserHelper.IsExpression(attribute.Value?.Value))
            {
                var symbol = ResolveAttributeValueBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, attribute);
                if (symbol != null)
                {
                    return symbol;
                }
            }

            return ResolveBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, attribute.Parent);
        }

        public ITypeSymbol ResolveBindingContext(IParsedXamlDocument xamlDocument,
                                                 IXamlSemanticModel semanticModel,
                                                 IXamlPlatform platform,
                                                 Project project,
                                                 Compilation compilation,
                                                 IXamlNamespaceCollection namespaces,
                                                 XmlNode node)
        {
            if (node == null)
            {
                return null;
            }

            var explicitBindingContext = ResolveChildNodeBindingContext(node, project, namespaces, xamlDocument.XmlnsDefinitions);
            if (explicitBindingContext != null)
            {
                return explicitBindingContext;
            }

            if (!node.HasAttribute("BindingContext") && !node.HasAttribute("x:DataType"))
            {
                if (node.IsRoot)
                {
                    return xamlDocument.BindingContext;
                }

                var parentNode = node.Parent;

                return ResolveParentNodeBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, parentNode);
            }

            var bindingContextAttribute = node.GetAttributeByName("BindingContext");
            var dataTypeAttribute = node.GetAttributeByName("x:DataType");

            if (dataTypeAttribute != null)
            {
                return ResolveDataType(project, namespaces, xamlDocument.XmlnsDefinitions, dataTypeAttribute);
            }

            if (!ExpressionParserHelper.IsExpression(bindingContextAttribute.Value?.Value))
            {
                return xamlDocument.BindingContext;
            }

            return ResolveAttributeValueBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, bindingContextAttribute);
        }

        ITypeSymbol ResolveDataType(Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, XmlAttribute dataTypeAttribute)
        {
            var symbol = dataTypeAttribute?.Value?.Value;

            return ResolveType(project, namespaces, xmlnsDefinitions, symbol);
        }

        ITypeSymbol ResolveType(Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, string xamlSymbolName)
        {
            if (string.IsNullOrEmpty(xamlSymbolName))
            {
                return default;
            }

            if (!XamlSyntaxHelper.ExplodeTypeReference(xamlSymbolName, out var xmlns, out var className))
            {
                return default;
            }

            var xmlNamespace = namespaces.ResolveNamespace(xmlns);

            return XamlTypeResolver.ResolveType(className, xmlNamespace, project, xmlnsDefinitions);
        }

        public ITypeSymbol ResolveChildNodeBindingContext(XmlNode targetNode, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (targetNode == null)
            {
                return null;
            }

            if (targetNode.HasChildNamed(targetNode.Name.FullName + ".BindingContext"))
            {
                var node = targetNode.GetChildNode(targetNode.Name.FullName + ".BindingContext");

                if (!node.HasChildren)
                {
                    return null;
                }

                var symbol = GetNamedTypeSymbol(node.Children.First(), project, namespaces, xmlnsDefinitions);

                return symbol;
            }

            return null;
        }

        public ITypeSymbol ResolveBindingContext(IParsedXamlDocument xamlDocument,
                                                 IXamlSemanticModel semanticModel,
                                                 IXamlPlatform platform,
                                                 Project project,
                                                 Compilation compilation,
                                                 IXamlNamespaceCollection namespaces,
                                                 BindingExpression expression,
                                                 XmlNode expressionParentNode)
        {
            var bindingSource = expression.Source;
            if (bindingSource != null)
            {
                if (bindingSource.IsReferenceValue)
                {
                    return ResolveReferenceSymbol(xamlDocument, project, compilation, namespaces, bindingSource.Reference);
                }

                var result = ExpressionEvaluater.Evaluate(xamlDocument, semanticModel, platform, project, compilation, namespaces, bindingSource.AssignmentValue);

                if (result != null && result.IsSymbolType<ITypeSymbol>())
                {
                    var source = result.GetSymbol<ITypeSymbol>();
                    var member = source.GetMembers(expression.ReferencedSymbolValue).FirstOrDefault();

                    if (member != null)
                    {
                        return SymbolHelper.ResolveMemberReturnType(member);
                    }
                }
            }

            if (TryResolveSpecialExpressionBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, expression, expressionParentNode, out var specialBindingContext))
            {
                return specialBindingContext;
            }

            var explicitNodeBindingContext = ResolveChildNodeBindingContext(expressionParentNode, project, namespaces, xamlDocument.XmlnsDefinitions);

            if (expressionParentNode.HasAttribute("x:DataType"))
            {
                var dataTypeAttribute = expressionParentNode.GetAttributeByName("x:DataType");

                return ResolveDataType(project, namespaces, xamlDocument.XmlnsDefinitions, dataTypeAttribute);
            }
            else if (expressionParentNode.HasAttribute(platform.BindingContextProperty))
            {
                var bindingContextAttr = expressionParentNode.GetAttributeByName(platform.BindingContextProperty);

                // Binding expression onto the "BindingContext" attribute, resolve 
                if (bindingContextAttr == expression.ParentAttribute)
                {
                    // Check for an explicit binding context setter
                    var parentBindingContext = explicitNodeBindingContext;
                    if (explicitNodeBindingContext == null
                        && !expressionParentNode.IsRoot)
                    {
                        parentBindingContext = ResolveBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, expressionParentNode.Parent);
                    }

                    return parentBindingContext;
                }
                else if (ExpressionParserHelper.IsExpression(bindingContextAttr.Value?.Value)
                         && ExpressionEvaluater.CanEvaluate(bindingContextAttr, project, namespaces, xamlDocument.XmlnsDefinitions, platform))
                {
                    var result = ExpressionEvaluater.Evaluate(xamlDocument, semanticModel, platform, project, compilation, namespaces, bindingContextAttr);

                    if (result == null)
                    {
                        return null;
                    }

                    if (result.Symbol is IPropertySymbol property)
                    {
                        return property.Type;
                    }
                    else if (result.Symbol is IFieldSymbol field)
                    {
                        return field.Type;
                    }
                    else if (result.Symbol is INamedTypeSymbol type)
                    {
                        return type;
                    }

                    return null;
                }

                return ResolveBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, bindingContextAttr);
            }

            if (explicitNodeBindingContext != null)
            {
                return explicitNodeBindingContext;
            }

            // Check if the parent node has a binding context.

            if (expressionParentNode.IsRoot)
            {
                return xamlDocument.BindingContext;
            }

            var parentNode = expressionParentNode.Parent;

            return ResolveParentNodeBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, parentNode);
        }

        INamedTypeSymbol GetNamedTypeSymbol(XmlNode node, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (node == null || project == null || namespaces == null || xmlnsDefinitions is null)
            {
                return null;
            }

            var xmlns = namespaces.ResolveNamespace(node);

            return XamlTypeResolver.ResolveType(node.Name.LocalName, xmlns, project, xmlnsDefinitions);
        }

        ITypeSymbol ResolveParentNodeBindingContext(IParsedXamlDocument xamlDocument,
                                                    IXamlSemanticModel semanticModel,
                                                    IXamlPlatform platform,
                                                    Project project,
                                                    Compilation compilation,
                                                    IXamlNamespaceCollection namespaces,
                                                    XmlNode node)
        {
            XmlNode parentDataTemplate = null;

            var parentNode = node;

            // Walk up to the root node and figure out the chain of command
            while (parentNode != null)
            {
                if (parentNode != null)
                {
                    if (parentNode.HasAttribute("x:DataType"))
                    {
                        var dataTypeAttribute = parentNode.GetAttributeByName("x:DataType");

                        return ResolveDataType(project, namespaces, xamlDocument.XmlnsDefinitions, dataTypeAttribute);
                    }
                    else if (parentNode.HasAttribute(platform.BindingContextProperty))
                    {
                        var bindingContextAttr = parentNode.GetAttributeByName(platform.BindingContextProperty);

                        return ResolveBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, bindingContextAttr);
                    }
                    else if (parentNode.HasChildNamed($"{parentNode.Name.FullName}.{platform.BindingContextProperty}"))
                    {
                        var bindingContextChild = parentNode.GetChildNode($"{parentNode.Name.FullName}.{platform.BindingContextProperty}");

                        if (!bindingContextChild.HasChildren)
                        {
                            return null;
                        }

                        var symbol = GetNamedTypeSymbol(bindingContextChild.Children.FirstOrDefault(), project, namespaces, xamlDocument.XmlnsDefinitions);

                        return symbol;
                    }
                    else if (parentNode.Name.LocalName == platform.DataTemplate.Name)
                    {
                        parentDataTemplate = parentNode.Parent?.Parent;
                        break;
                    }
                }

                parentNode = parentNode.Parent;
            }

            if (parentDataTemplate == null)
            {
                return xamlDocument.BindingContext;
            }

            var isItemTemplate = true;
            if (parentNode.HasParent)
            {
                var elementName = parentNode.Parent.Name.LocalName;
                if (XamlSyntaxHelper.IsPropertySetter(parentNode.Parent)
                    && XamlSyntaxHelper.ExplodePropertySetter(parentNode.Parent, out _, out var parentPropertyName))
                {
                    elementName = parentPropertyName;
                }

                isItemTemplate = elementName == platform.ItemTemplateProperty;
            }

            var bindingContext = ResolveDataTemplateBindingContext(xamlDocument, semanticModel, platform, project, compilation, namespaces, parentDataTemplate, isItemTemplate);

            return bindingContext;
        }

        INamedTypeSymbol ResolveDataTemplateBindingContext(IParsedXamlDocument xamlDocument,
                                                           IXamlSemanticModel semanticModel,
                                                           IXamlPlatform platform,
                                                           Project project,
                                                           Compilation compilation,
                                                           IXamlNamespaceCollection namespaces,
                                                           XmlNode parentDataTemplate,
                                                           bool isItemTemplate)
        {
            // Find the ItemsSource attribute.
            var itemsSourceAttr = parentDataTemplate.GetAttribute(attr => attr.Name.LocalName == platform.ItemSourceProperty);

            if (itemsSourceAttr is null && platform.SupportsBindableLayout)
            {
                var bindableLayoutItemSourceProperty = platform.BindableLayout.MetaType.Split('.').Last() + "." + platform.ItemSourceProperty;
                itemsSourceAttr = parentDataTemplate.GetAttribute(attr => attr.Name.LocalName == bindableLayoutItemSourceProperty);
            }

            if (itemsSourceAttr == null)
            {
                return null;
            }

            ISymbol targetSymbol = null;
            if (ExpressionEvaluater.CanEvaluate(itemsSourceAttr, project, namespaces, xamlDocument.XmlnsDefinitions, platform))
            {
                var result = ExpressionEvaluater.Evaluate(xamlDocument, semanticModel, platform, project, compilation, namespaces, itemsSourceAttr);

                if (result == null || result.Symbol == null)
                {
                    return null;
                }

                targetSymbol = result.GetSymbol<ISymbol>();
            }

            if (targetSymbol == null)
            {
                return null;
            }

            INamedTypeSymbol bindingContext = null;
            if (targetSymbol is IPropertySymbol property)
            {
                bindingContext = UnwrapCollectionType(property.Type as INamedTypeSymbol);
            }

            if (isItemTemplate)
            {
                var isGroupedAttr = parentDataTemplate.GetAttribute(attr => attr.Name.LocalName == "IsGroupingEnabled" || attr.Name.LocalName == "IsGrouped");

                if (string.Equals(isGroupedAttr?.Value?.Value, "true" , StringComparison.OrdinalIgnoreCase))
                {
                    bindingContext = UnwrapCollectionType(bindingContext.BaseType) ?? bindingContext;
                }
            }

            return bindingContext;
        }

        INamedTypeSymbol UnwrapCollectionType(INamedTypeSymbol type)
        {
            if (type == null)
            {
                return null;
            }

            if (type.ToString() == "System.Collections.IEnumerable")
            {
                return type;
            }
            else if (type.IsGenericType)
            {
                return type.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
            }
            else
            {
                // Array?
            }

            return null;
        }

        bool TryResolveSpecialExpressionBindingContext(IParsedXamlDocument xamlDocument,
                                                       IXamlSemanticModel semanticModel,
                                                       IXamlPlatform platform,
                                                       Project project,
                                                       Compilation compilation,
                                                       IXamlNamespaceCollection namespaces,
                                                       BindingExpression expression,
                                                       XmlNode expressionParentNode,
                                                       out ITypeSymbol result)
        {
            result = null;

            var outerType = ResolveType(project, namespaces, xamlDocument.XmlnsDefinitions, expression.ParentAttribute.Parent.Name.FullName);
            if (outerType == null)
            {
                return false;
            }

            var pickerType = compilation.GetTypeByMetadataName(platform.Picker.MetaType);
            var listViewType = compilation.GetTypeByMetadataName(platform.ListView.MetaType);
            var collectionViewType = compilation.GetTypeByMetadataName(platform.CollectionView.MetaType);

            var shouldInspectItemsSource = false;
            if (SymbolHelper.DerivesFrom(outerType, pickerType))
            {
                shouldInspectItemsSource = expression.ParentAttribute.Name.LocalName == platform.PickerItemDisplayBindingProperty;
            }
            else if (SymbolHelper.DerivesFrom(outerType, listViewType))
            {
                if (expression.ParentAttribute.Name.LocalName == platform.GroupDisplayBindingProperty)
                {
                    var isGroupingEnabled = expression.ParentAttribute.Parent.GetAttributeByName(platform.IsGroupingEnabledProperty);
                    if (string.Equals(isGroupingEnabled?.Value?.Value, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        shouldInspectItemsSource = true;
                    }
                }
                // Unsupported at the moment.
            }

            if (shouldInspectItemsSource)
            {
                var itemsSource = expressionParentNode.GetAttribute((XmlAttribute arg) => arg.Name.LocalName == platform.ItemSourceProperty);

                if (itemsSource != null)
                {
                    var eval = ExpressionEvaluater.Evaluate(xamlDocument, semanticModel, platform, project, compilation, namespaces, itemsSource);

                    if (eval != null && eval.Symbol != null)
                    {
                        var property = eval.Symbol as IPropertySymbol;

                        try
                        {
                            if (property != null)
                            {
                                result = UnwrapCollectionType(property.Type as INamedTypeSymbol);
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                        }
                    }
                }
            }

            return shouldInspectItemsSource;
        }
    }
}

