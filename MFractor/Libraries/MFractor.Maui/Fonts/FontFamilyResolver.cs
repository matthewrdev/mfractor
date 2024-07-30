using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Data;
using MFractor.Fonts;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Semantics;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Styles;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Xml;

namespace MFractor.Maui.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontFamilyResolver))]
    class FontFamilyResolver : IFontFamilyResolver
    {
        readonly Lazy<IStaticResourceResolver> staticResourceResolver;
        public IStaticResourceResolver StaticResourceResolver => staticResourceResolver.Value;

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater MarkupExpressionEvaluater => expressionEvaluater.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IStyleResolver> styleResolver;
        public IStyleResolver StyleResolver => styleResolver.Value;

        readonly Lazy<IFontAssetResolver> fontAssetResolver;
        public IFontAssetResolver FontAssetResolver => fontAssetResolver.Value;

        readonly Lazy<IEmbeddedFontsResolver> embeddedFontsResolver;
        public IEmbeddedFontsResolver EmbeddedFontsResolver => embeddedFontsResolver.Value;

        [ImportingConstructor]
        public FontFamilyResolver(Lazy<IStaticResourceResolver> staticResourceResolver,
                                  Lazy<IFontAssetResolver> fontAssetResolver,
                                  Lazy<IMarkupExpressionEvaluater> expressionEvaluater,
                                  Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                  Lazy<IStyleResolver> styleResolver,
                                  Lazy<IEmbeddedFontsResolver> embeddedFontsResolver)
        {
            this.staticResourceResolver = staticResourceResolver;
            this.fontAssetResolver = fontAssetResolver;
            this.expressionEvaluater = expressionEvaluater;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.styleResolver = styleResolver;
            this.embeddedFontsResolver = embeddedFontsResolver;
        }

        const string fontFamilyPropertyName = "FontFamily";

        public IFont ResolveFont(XmlNode xmlNode, IXamlSemanticModel semanticModel, IXamlPlatform platform, IProjectFile projectFile, bool searchProjectReferences = true)
        {
            if (xmlNode == null || projectFile == null)
            {
                return default;
            }

            var fontFamilyAttr = xmlNode.GetAttributeByName(fontFamilyPropertyName);

            if (fontFamilyAttr != null)
            {
                return ResolveFont(fontFamilyAttr, projectFile, platform, searchProjectReferences);
            }

            var fontFamilyNode = xmlNode.GetChildNode($"{xmlNode.Name.FullName}.{fontFamilyPropertyName}");
            if (fontFamilyNode != null)
            {
                return GetFontFromFontFamilyPropertyNode(projectFile, searchProjectReferences, fontFamilyNode);
            }

            var styleAttr = xmlNode.GetAttributeByName("Style");
            if (styleAttr != null)
            {
                return GetFontFamilyFromStyle(projectFile, platform, searchProjectReferences, styleAttr);
            }

            return default;
        }

        IFont GetFontFamilyFromStyle(IProjectFile projectFile, IXamlPlatform platform, bool searchProjectReferences, XmlAttribute styleAttr)
        {
            var styleName = GetStaticResourceNameFromExpression(styleAttr?.Value?.Value);

            var style = StyleResolver.ResolveStyleByName(projectFile.CompilationProject, platform, projectFile.FilePath, styleName);

            if (style == null)
            {
                return default;
            }

            var fontFamily = style.Properties.FirstOrDefault(p => p.Name == "FontFamily");

            if (fontFamily == null
                || !(fontFamily.Value is ILiteralStylePropertyValue propertyValue))
            {
                return default;
            }

            return ResolveFontFromContent(propertyValue.Value, projectFile, platform, searchProjectReferences);
        }

        string GetStaticResourceNameFromExpression(string value)
        {
            var parser = new XamlExpressionParser(value, 0);
            var expression = parser.Parse();

            if (expression == null)
            {
                return default;
            }

            if (expression is ExpressionSyntax expressionSyntax)
            {
                var nameSyntax = expressionSyntax.NameSyntax;

                if (nameSyntax == null)
                {
                    return string.Empty;
                }

                var expressionName = string.Empty;
                if (nameSyntax is SymbolSyntax symbol)
                {
                    expressionName = symbol.TypeNameSyntax.Name;
                }
                else if (nameSyntax is TypeNameSyntax typeNameSyntax)
                {
                    expressionName = typeNameSyntax.Name;
                }

                if (expressionName != "StaticResource")
                {
                    return string.Empty;
                }

                return expressionSyntax.Elements.OfType<ContentSyntax>().FirstOrDefault()?.Content;
            }

            return string.Empty;
        }

        IFont GetFontFromFontFamilyPropertyNode(IProjectFile projectFile, bool searchProjectReferences, XmlNode fontFamilyNode)
        {
            if (fontFamilyNode.HasChildren)
            {
                var child = fontFamilyNode.Children.First();

                if (!child.HasValue)
                {
                    return default;
                }

                return FontAssetResolver.GetFontAssetsWithPostscriptName(projectFile.CompilationProject, child.Value, searchProjectReferences).FirstOrDefault();
            }

            if (!fontFamilyNode.HasValue)
            {
                return default;
            }

            return FontAssetResolver.GetFontAssetsWithPostscriptName(projectFile.CompilationProject, fontFamilyNode.Value, searchProjectReferences).FirstOrDefault();
        }

        public IFont ResolveFont(XmlAttribute syntax, IProjectFile projectFile, IXamlPlatform platform, bool searchProjectReferences)
        {
            if (!syntax.HasValue)
            {
                return default;
            }

            var value = syntax.Value.Value;

            return ResolveFontFromContent(value, projectFile, platform, searchProjectReferences);
        }

        IFont ResolveFontFromContent(string value, IProjectFile projectFile, IXamlPlatform platform, bool searchProjectReferences)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            if (ExpressionParserHelper.IsExpression(value))
            {
                var resourceName = GetStaticResourceNameFromExpression(value);

                if (string.IsNullOrEmpty(resourceName))
                {
                    return default;
                }

                return ResolveFontByStaticResourceName(resourceName, projectFile, platform);
            }

            var embeddedFonts = EmbeddedFontsResolver.GetEmbeddedFonts(projectFile.CompilationProject, platform);

            if (embeddedFonts.Any())
            {
                var embeddedFont = embeddedFonts.FirstOrDefault(ef => ef.FontFileName == value
                                                                      || ef.FontName == value
                                                                      || ef.Alias == value);

                if (embeddedFont != null)
                {
                    return embeddedFont.Font;
                }
            }

            return FontAssetResolver.GetFontAssetsWithPostscriptName(projectFile.CompilationProject, value, searchProjectReferences).FirstOrDefault();
        }

        IFont ResolveFontByStaticResourceName(string resourceName, IProjectFile projectFile, IXamlPlatform platform)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            var definitions = StaticResourceResolver.FindNamedStaticResources(projectFile.CompilationProject, platform, projectFile.FilePath, resourceName);

            if (definitions == null || !definitions.Any())
            {
                return null;
            }

            var definition = definitions.FirstOrDefault();

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(projectFile.CompilationProject);

            if (database != null)
            {
                var onPlatformRepo = database.GetRepository<OnPlatformDeclarationRepository>();

                var onPlatform = onPlatformRepo.GetOnPlatformDeclarationForStaticResourceDefinition(definition);

                if (onPlatform != null
                    && onPlatform.Type == "System.String"
                    && onPlatform.HasPlatforms
                    && onPlatform.Platforms.TryGetValue("iOS", out var postscriptName))
                {
                    return FontAssetResolver.GetFontAssetsWithPostscriptName(projectFile.CompilationProject, postscriptName)?.FirstOrDefault();
                }
            }

            return default;
        }
    }
}