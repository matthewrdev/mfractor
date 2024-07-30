using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Fonts;
using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Images;
using MFractor.Localisation;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Symbols
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlSymbolResolver))]
    class XamlSymbolResolver : IXamlSymbolResolver
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluator;
        IMarkupExpressionEvaluater ExpressionEvaluator => expressionEvaluator.Value;

        readonly Lazy<IImageAssetService> imageAssetService;
        IImageAssetService ImageAssetService => imageAssetService.Value;

        readonly Lazy<IEmbeddedFontsResolver> embeddedFontsResolver;
        IEmbeddedFontsResolver EmbeddedFontsResolver => embeddedFontsResolver.Value;

        readonly Lazy<IXmlSyntaxFinder> xmlSyntaxFinder;
        public IXmlSyntaxFinder XmlSyntaxFinder => xmlSyntaxFinder.Value;

        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<ILocalisationResolver> localisationResolver;
        public ILocalisationResolver LocalisationResolver => localisationResolver.Value;

        [ImportingConstructor]
        public XamlSymbolResolver(Lazy<IImageAssetService> imageAssetService,
                                  Lazy<IMarkupExpressionEvaluater> expressionEvaluater,
                                  Lazy<IXmlSyntaxFinder> xmlSyntaxFinder,
                                  Lazy<IEmbeddedFontsResolver> embeddedFontsResolver,
                                  Lazy<IXamlTypeResolver> xamlTypeResolver,
                                  Lazy<IProjectService> projectService,
                                  Lazy<ILocalisationResolver> localisationResolver)
        {
            this.imageAssetService = imageAssetService;
            this.expressionEvaluator = expressionEvaluater;
            this.xmlSyntaxFinder = xmlSyntaxFinder;
            this.embeddedFontsResolver = embeddedFontsResolver;
            this.xamlTypeResolver = xamlTypeResolver;
            this.projectService = projectService;
            this.localisationResolver = localisationResolver;
        }

        public XamlSymbolInfo Resolve(IParsedXamlDocument document,
                                      IXamlSemanticModel semanticModel,
                                      IXamlPlatform platform,
                                      Project project,
                                      Compilation compilation,
                                      IXamlNamespaceCollection namespaces,
                                      int position)
        {
            var path = XmlSyntaxFinder.BuildXmlPathToOffset(document.XamlSyntaxTree, position, out _);

            if (path.Count == 0)
            {
                return new XamlSymbolInfo(); // Empty result, unresolvable.
            }

            return Resolve(document, semanticModel, platform, project, compilation, namespaces, path.Last(), position);
        }

        public XamlSymbolInfo Resolve(IParsedXamlDocument document,
                                      IXamlSemanticModel semanticModel,
                                      IXamlPlatform platform,
                                      Project project,
                                      Compilation compilation,
                                      IXamlNamespaceCollection namespaces,
                                      XmlSyntax syntax,
                                      int? position = null)
        {
            XamlSymbolInfo result = null;
            if (syntax is XmlNode node)
            {
                result = ResolveXamlNode(document, project, compilation, platform, namespaces, node);

                if (position != null
                    && result != null
                    && node.HasValue
                    && FileLocationHelper.IsBetween(position.Value, node.ValueSpan))
                {
                    return ResolveXamlNodeValue(project, node, result.Symbol as ISymbol, platform);
                }
            }
            else if (syntax is XmlAttribute attribute)
            {
                result = ResolveAttribute(document, project, compilation, platform, namespaces, attribute);

                if (result == null)
                {
                    return null;
                }

                var attributeSymbol = result.Symbol as ISymbol;

                if (position != null && attribute.HasValue)
                {
                    if (FileLocationHelper.IsBetween(position.Value, attribute.Value.Span))
                    {
                        return ResolveAttributeValue(document, semanticModel, platform, project, compilation, namespaces, attribute, attributeSymbol);
                    }
                }
            }

            return result;
        }

        XamlSymbolInfo ResolveXamlNodeValue(Project project,
                                            XmlNode node,
                                            ISymbol nodeSymbol,
                                            IXamlPlatform platform)
        {
            if (nodeSymbol is INamedTypeSymbol namedType)
            {
                if (FormsSymbolHelper.IsColor(namedType, platform))
                {
                    if (ColorHelper.TryEvaluateColor(node.Value, out var color))
                    {
                        return new XamlSymbolInfo()
                        {
                            Symbol = color,
                            Syntax = node,
                            Span = node.ValueSpan,
                            SymbolKind = XamlSymbolKind.Color,
                        };
                    }
                }
                else if (ImageHelper.IsImageFile(node.Value))
                {
                    var asset = ImageAssetService.FindImageAsset(node.Value, project);
                    if (asset != null)
                    {
                        return new XamlSymbolInfo()
                        {
                            Symbol = asset,
                            Syntax = node,
                            Span = node.ValueSpan,
                            SymbolKind = XamlSymbolKind.Image,
                        };
                    }
                }
            }
            else if (nodeSymbol is IPropertySymbol property)
            {
                if (SymbolHelper.DerivesFrom(property.Type, platform.ImageSource.MetaType))
                {
                    var asset = ImageAssetService.FindImageAsset(node.Value, project);
                    if (asset != null)
                    {
                        return new XamlSymbolInfo()
                        {
                            Symbol = asset,
                            Syntax = node,
                            Span = node.ValueSpan,
                            SymbolKind = XamlSymbolKind.Image,
                        };
                    }
                }
            }

            return null;
        }

        public XamlSymbolInfo ResolveXamlNode(IParsedXamlDocument document, Project project, Compilation compilation, IXamlPlatform platform, IXamlNamespaceCollection namespaces, XmlNode element)
        {
            var isKnownGeneric = element.HasAttribute(attr => XamlSyntaxHelper.IsTypeArguments(attr, namespaces));

            var resolvedSymbol = ResolveSymbol(document, project, compilation, platform, namespaces, element.Name.FullName, isKnownGeneric);
            if (resolvedSymbol == null)
            {

                // Try again, reversing the generic search...
                resolvedSymbol = ResolveSymbol(document, project, compilation, platform, namespaces, element.Name.FullName, !isKnownGeneric);

                if (resolvedSymbol == null)
                {
                    return null;
                }
            }

            var result = new XamlSymbolInfo
            {
                Symbol = resolvedSymbol,
                Syntax = element,
                Span = element.Span
            };

            return result;
        }

        public ISymbol ResolveSymbol(IParsedXamlDocument document,
                                     Project project,
                                     Compilation compilation,
                                     IXamlPlatform platform,
                                     IXamlNamespaceCollection namespaces,
                                     string elementFullName,
                                     bool isGeneric)
        {
            var elementNamespace = "";
            var elementSymbolName = elementFullName;
            if (elementFullName.Contains(":"))
            {
                var split = elementFullName.Split(':');
                elementNamespace = split[0];
                elementSymbolName = split[1];
            }

            var xamlNamespace = namespaces.ResolveNamespace(elementNamespace);
            if (xamlNamespace == null)
            {
                return null;
            }

            var classPath = elementSymbolName.Split('.');

            var typeSymbol = XamlTypeResolver.ResolveType(classPath[0], xamlNamespace, project, document.XmlnsDefinitions);

            if (typeSymbol == null)
            {
                return null;
            }

            if (classPath.Length > 1)
            {
                return ResolveMember(compilation, platform, typeSymbol, classPath[1]);
            }

            return typeSymbol;
        }

        public XamlSymbolInfo ResolveAttribute(IParsedXamlDocument document, Project project, Compilation compilation, IXamlPlatform platform, IXamlNamespaceCollection namespaces, XmlAttribute attribute)
        {
            var element = ResolveXamlNode(document, project, compilation, platform, namespaces, attribute.Parent);

            return ResolveAttribute(document, project, compilation, platform, namespaces, element, attribute);
        }

        public XamlSymbolInfo ResolveAttribute(IParsedXamlDocument document, Project project, Compilation compilation, IXamlPlatform platform, IXamlNamespaceCollection namespaces, XamlSymbolInfo elementResult, XmlAttribute attribute)
        {
            if (elementResult == null)
            {
                return null;
            }

            var typeSymbol = elementResult.Symbol as ITypeSymbol;
            if (typeSymbol == null)
            {

                if (elementResult.Symbol is IPropertySymbol)
                {
                    typeSymbol = (elementResult.Symbol as IPropertySymbol).Type;
                }
                else if (elementResult.Symbol is IFieldSymbol)
                {
                    typeSymbol = (elementResult.Symbol as IFieldSymbol).Type;
                }

                if (typeSymbol == null)
                {
                    return null;
                }
            }

            return new XamlSymbolInfo()
            {
                Symbol = ResolveSymbol(document, project, compilation, platform, namespaces, typeSymbol, attribute.Name.FullName),
                Syntax = attribute,
                Span = attribute.Span
            };
        }

        public ISymbol ResolveSymbol(IParsedXamlDocument document,
            Project project,
            Compilation compilation,
            IXamlPlatform platform,
            IXamlNamespaceCollection namespaces,
            ITypeSymbol target,
            string symbolName)
        {
            // Check if the symbol is an Attached Property...
            var parentType = target;
            var searchableSymbolName = symbolName;
            if (symbolName.Contains(".")) // Boom, this is an attached property
            {
                var split = symbolName.Split('.');
                var parentName = string.Join(".", split.Take(split.Length - 1));

                // Resolve the parent type.
                parentType = ResolveSymbol(document, project, compilation, platform, namespaces, parentName, false) as INamedTypeSymbol;

                if (parentType == null)
                {
                    return null;
                }

                searchableSymbolName = split.Last() + "Property";
            }
            else if (symbolName.Contains(":"))
            {
                XamlSyntaxHelper.ExplodeTypeReference(symbolName, out var @xmlns, out var name);

                var @namespace = namespaces.ResolveNamespace(@xmlns);

                if (XamlSchemaHelper.IsSchema(@namespace, XamlSchemas.DesignSchemaUrl))
                {
                    searchableSymbolName = name;
                }
            }

            return ResolveMember(compilation, platform, parentType, searchableSymbolName);
        }

        ISymbol ResolveMember(Compilation compilation, IXamlPlatform platform, ITypeSymbol type, string propertyName)
        {
            if (propertyName == "CachingStrategy")
            {
                var listView = compilation.GetTypeByMetadataName(platform.ListView.MetaType);

                if (SymbolHelper.DerivesFrom(type, listView))
                {
                    var members = listView.GetMembers();
                    var cachingStrategy = members.FirstOrDefault(m => m.Name.EndsWith("CachingStrategy", StringComparison.Ordinal));
                    return cachingStrategy;
                }
            }

            var baseType = type.BaseType;

            var symbol = type.GetMembers(propertyName).FirstOrDefault(m => m is IPropertySymbol || m is IEventSymbol || m is IFieldSymbol);

            if (symbol == null)
            {
                baseType = type.BaseType;

                while (baseType != null)
                {
                    // Resolve the base type symbols.
                    symbol = baseType.GetMembers(propertyName).FirstOrDefault(m => m is IPropertySymbol || m is IEventSymbol || m is IFieldSymbol);
                    if (symbol == null)
                    {
                        baseType = baseType.BaseType;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (symbol == null)
            {
                symbol = type.GetMembers(propertyName + "Property").OfType<IFieldSymbol>().FirstOrDefault(m => m.IsStatic);
            }

            return symbol;
        }

        public XamlSymbolInfo ResolveAttributeValue(IParsedXamlDocument document,
                                                    IXamlSemanticModel semanticModel,
                                                    IXamlPlatform platform,
                                                    Project project,
                                                    Compilation compilation,
                                                    IXamlNamespaceCollection namespaces,
                                                    XmlAttribute attribute,
                                                    ISymbol attributeSymbol)
        {
            var value = attribute.Value;

            var span = default(TextSpan);
            ISymbol symbol = null;
            Expression parsedExpression = null;
            var returnType = SymbolHelper.ResolveMemberReturnType(attributeSymbol);

            if (attribute.HasValue)
            {
                span = attribute.Value.Span;

                if (ExpressionEvaluator.CanEvaluate(attribute, project, namespaces, document.XmlnsDefinitions, platform))
                {
                    var expressionResult = ExpressionEvaluator.Evaluate(document, semanticModel, platform, project, compilation, namespaces, attribute);
                    if (expressionResult != null)
                    {
                        if (expressionResult.Expression is StaticBindingExpression
                            && expressionResult.Symbol is IPropertySymbol propertySymbol)
                        {
                            var localisationResult = EvaluateLocalisationExpression(project, propertySymbol, expressionResult);

                            if (localisationResult != null)
                            {
                                return localisationResult;
                            }
                        }

                        return expressionResult;
                    }
                }
                else
                {
                    var property = attributeSymbol as IPropertySymbol;
                    if (XamlSyntaxHelper.IsCodeBehindFieldName(attribute, namespaces))
                    {
                        symbol = ResolveCodeBehindSymbol(document.CodeBehindSymbol, value.Value);
                    }
                    else if (FormsSymbolHelper.IsColor(property?.Type, platform))
                    {
                        if (ColorHelper.TryEvaluateColor(attribute.Value?.Value, out var color))
                        {
                            return new XamlSymbolInfo()
                            {
                                Symbol = color,
                                Syntax = attribute,
                                Span = span,
                                SymbolKind = XamlSymbolKind.Color,
                            };
                        }
                    }
                    else if (XamlSyntaxHelper.IsDictionaryKey(attribute, namespaces))
                    {

                    }
                    else if (SymbolHelper.DerivesFrom(SymbolHelper.ResolveMemberReturnType(attributeSymbol), platform.ImageSource.MetaType))
                    {
                        var asset = ImageAssetService.FindImageAsset(value.Value, project);
                        if (asset != null)
                        {
                            return new XamlSymbolInfo()
                            {
                                Symbol = asset,
                                Syntax = attribute,
                                Span = span,
                                SymbolKind = XamlSymbolKind.Image,
                            };
                        }
                    }
                    else if (SymbolHelper.DerivesFrom(attributeSymbol?.ContainingType as INamedTypeSymbol, platform.Setter.MetaType))
                    {
                        return ResolveSetter(attribute, document.CodeBehindSymbol, project, compilation, platform, namespaces, document.XmlnsDefinitions);
                    }
                    else if (SymbolHelper.DerivesFrom(SymbolHelper.ResolveMemberReturnType(attributeSymbol), "System.Type"))
                    {
                        symbol = GetNamedTypeSymbol(attribute.Value?.Value, project, namespaces, document.XmlnsDefinitions);
                    }
                    else if (property != null
                             && property.Name == "Value"
                             && SymbolHelper.DerivesFrom(property.ContainingType, platform.Setter.MetaType))
                    {
                        if (ColorHelper.TryEvaluateColor(attribute.Value?.Value, out var color))
                        {
                            return new XamlSymbolInfo()
                            {
                                Symbol = color,
                                Syntax = attribute,
                                Span = span,
                                SymbolKind = XamlSymbolKind.Color,
                            };
                        }

                        if (ImageHelper.IsImageFile(value.Value))
                        {
                            var asset = ImageAssetService.FindImageAsset(value.Value, project);
                            if (asset != null)
                            {
                                return new XamlSymbolInfo()
                                {
                                    Symbol = asset,
                                    Syntax = attribute,
                                    Span = span,
                                    SymbolKind = XamlSymbolKind.Image,
                                };
                            }
                        }
                    }
                    else if (IsShapeGeometry(property, attribute, platform))
                    {
                        // TODO: Native SVG rendering
                    }
                    else if (attribute.Name.FullName == "FontFamily"
                             && returnType.SpecialType == SpecialType.System_String)
                    {
                        var exportFontAttribute = compilation.GetTypeByMetadataName(platform.ExportFontAttribute.MetaType);

                        if (exportFontAttribute != null)
                        {
                            var embeddedFont = EmbeddedFontsResolver.GetEmbeddedFontByName(project, platform, attribute.Value.Value);

                            if (embeddedFont != null)
                            {
                                return new XamlSymbolInfo()
                                {
                                    Symbol = embeddedFont.Font,
                                    Syntax = attribute,
                                    Span = span,
                                    SymbolKind = XamlSymbolKind.Font,
                                };
                            }
                        }
                    }
                    else
                    {
                        symbol = ResolveReferencedAttributeValueSymbolByName(document.CodeBehindSymbol, attributeSymbol, value?.Value);
                    }
                }
            }

            var result = new XamlSymbolInfo
            {
                Symbol = symbol,
                Syntax = attribute,
                Span = span,
                Expression = parsedExpression
            };

            return result;
        }

        XamlSymbolInfo EvaluateLocalisationExpression(Project project, IPropertySymbol propertySymbol, XamlSymbolInfo expressionResult)
        {
            var localisations = LocalisationResolver.ResolveLocalisations(project, propertySymbol);

            if (localisations is null || !localisations.Any())
            {
                return default;
            }

            return new XamlSymbolInfo()
            {
                SymbolKind = XamlSymbolKind.Localisation,
                Expression = expressionResult.Expression,
                Span = expressionResult.Span,
                Symbol = localisations,
            };
        }

        bool IsShapeGeometry(IPropertySymbol property, XmlAttribute attribute, IXamlPlatform platform)
        {
            if (!platform.SupportsGeometry)
            {
                return false;
            }

            if (property is null)
            {
                return false;
            }

            var propertyType = property.ContainingType;

            var isGeometry = SymbolHelper.DerivesFrom(propertyType, platform.PathFigureCollection.MetaType)
                            || SymbolHelper.DerivesFrom(propertyType, platform.Path.MetaType)
                            || SymbolHelper.DerivesFrom(propertyType, platform.Geometry.MetaType);

            if (!isGeometry)
            {
                return false;
            }

            if (ExpressionParserHelper.IsExpression(attribute?.Value?.Value))
            {
                return false;
            }

            return true;
        }

        public XamlSymbolInfo ResolveSetter(XmlAttribute attribute,
                                            INamedTypeSymbol codeBehind,
                                            Project project,
                                            Compilation compilation,
                                            IXamlPlatform platform,
                                            IXamlNamespaceCollection namespaces,
                                            IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (attribute == null || !attribute.HasValue)
            {
                return null;
            }

            var targetTypeAttr = attribute.Parent?.Parent?.GetAttributeByName("TargetType");

            if (targetTypeAttr == null
            || !targetTypeAttr.HasValue)
            {
                return null;
            }

            var targetType = GetNamedTypeSymbol(targetTypeAttr.Value.Value, project, namespaces, xmlnsDefinitions);
            if (targetType == null)
            {
                return null;
            }

            ISymbol symbol = null;

            if (attribute.Name.LocalName == "Property")
            {
                symbol = SymbolHelper.FindMemberSymbolByName(targetType, attribute.Value.Value);
            }
            else if (attribute.Name.LocalName == "Value")
            {
                var propertyAttribute = attribute.Parent.GetAttributeByName("Property");

                if (propertyAttribute is null)
                {
                    return default;
                }

                IPropertySymbol property = default;
                if (XamlSyntaxHelper.IsPropertySetter(propertyAttribute.Value?.Value))
                {
                    XamlSyntaxHelper.ExplodePropertySetter(propertyAttribute.Value?.Value, out var className, out var propertyName);

                    var setterPropertyType = GetNamedTypeSymbol(className, project, namespaces, xmlnsDefinitions);

                    property = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(setterPropertyType, propertyName);
                }
                else
                {
                    property = ResolveSetter(propertyAttribute, codeBehind, project, compilation, platform, namespaces, xmlnsDefinitions)?.GetSymbol<IPropertySymbol>();
                }

                if (property != null && attribute.HasValue)
                {
                    if (SymbolHelper.DerivesFrom(property.Type, platform.ImageSource.MetaType))
                    {
                        var asset = ImageAssetService.FindImageAsset(attribute.Value.Value, project);
                        if (asset != null)
                        {
                            return new XamlSymbolInfo()
                            {
                                Symbol = asset,
                                Syntax = attribute,
                                Span = attribute.Value.Span,
                                SymbolKind = XamlSymbolKind.Image,
                            };
                        }
                    }
                    if (FormsSymbolHelper.IsColor(property.Type, platform))
                    {
                        if (ColorHelper.TryEvaluateColor(attribute.Value.Value, out var color))
                        {
                            return new XamlSymbolInfo()
                            {
                                Symbol = color,
                                Syntax = attribute,
                                Span = attribute.Value.Span,
                                SymbolKind = XamlSymbolKind.Color,
                            };
                        }
                    }

                    symbol = ResolveReferencedAttributeValueSymbolByName(codeBehind, property, attribute.Value.Value);
                }
            }

            if (symbol == null)
            {
                return null;
            }

            var result = new XamlSymbolInfo
            {
                Symbol = symbol,
                Syntax = attribute,
                Span = attribute.Value.Span
            };

            return result;
        }

        public ISymbol ResolveCodeBehindSymbol(INamedTypeSymbol codeBehindClass, string nameToResolve)
        {
            ISymbol symbol = SymbolHelper.FindMemberSymbolByName<IFieldSymbol>(codeBehindClass, nameToResolve);
            if (symbol == null)
            {
                symbol = SymbolHelper.FindMemberSymbolByName<IPropertySymbol>(codeBehindClass, nameToResolve);
            }

            return symbol;
        }

        public ISymbol ResolveReferencedAttributeValueSymbolByName(INamedTypeSymbol codeBehindClass, ISymbol referencedSymbol, string nameToResolve)
        {
            if (string.IsNullOrEmpty(nameToResolve))
            {
                return null;
            }

            ISymbol symbol = null;

            if (referencedSymbol is IPropertySymbol property)
            {
                var searchName = nameToResolve;
                if (nameToResolve.Contains("."))
                {
                    var components = nameToResolve.Split('.');
                    if (components.Length == 2)
                    {
                        if (components[0] == property.Type.Name)
                        {
                            searchName = components[1];
                        }
                    }
                }

                symbol = SymbolHelper.FindMemberSymbolByName(property.Type, searchName);
            }
            else if (referencedSymbol is IEventSymbol)
            {
                symbol = SymbolHelper.FindMemberSymbolByName<IEventSymbol>(codeBehindClass, nameToResolve);
                if (symbol == null)
                {
                    symbol = SymbolHelper.FindMemberSymbolByName<IMethodSymbol>(codeBehindClass, nameToResolve);
                }
            }

            return symbol;
        }

        public INamedTypeSymbol ResolveMarkupExtension(Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, NameSyntax nameSyntax)
        {
            var xmlns = namespaces.ResolveNamespace(nameSyntax);

            if (xmlns == null)
            {
                return null;
            }

            string name;
            if (nameSyntax is SymbolSyntax symbol)
            {
                if (symbol.TypeNameSyntax == null)
                {
                    return null;
                }
                else
                {
                    name = symbol.TypeNameSyntax.Name;
                }

            }
            else if (nameSyntax is TypeNameSyntax typeName)
            {
                name = typeName.Name;
            }
            else
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            name += "Extension";

            return XamlTypeResolver.ResolveType(name, xmlns, project, xmlnsDefinitions);
        }

        INamedTypeSymbol GetNamedTypeSymbol(string xamlName, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (string.IsNullOrEmpty(xamlName)
               || project == null
               || xmlnsDefinitions == null
               || namespaces == null)
            {
                return null;
            }

            if (!XamlSyntaxHelper.ExplodeTypeReference(xamlName, out var namespaceName, out var className))
            {
                return null;
            }

            var xmlns = namespaces.ResolveNamespace(namespaceName);

            return XamlTypeResolver.ResolveType(className, xmlns, project, xmlnsDefinitions);
        }

        INamedTypeSymbol GetNamedTypeSymbol(NameSyntax nameSyntax,
                                            Project project,
                                            IXamlNamespaceCollection namespaces,
                                            IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (nameSyntax == null
               || project == null
               || namespaces == null
               || xmlnsDefinitions == null)
            {
                return null;
            }

            var xmlns = namespaces.ResolveNamespace(nameSyntax);

            if (xmlns == null)
            {
                return null;
            }

            if (nameSyntax is NamespaceSyntax)
            {
                return null;
            }

            if (nameSyntax is SymbolSyntax symbolSyntax)
            {
                return GetNamedTypeSymbol(symbolSyntax.TypeNameSyntax, project, namespaces, xmlnsDefinitions);
            }

            var typeName = nameSyntax as TypeNameSyntax;

            if (typeName == null)
            {
                return null;
            }

            return XamlTypeResolver.ResolveType(typeName.Name, xmlns, project, xmlnsDefinitions);
        }
    }
}

