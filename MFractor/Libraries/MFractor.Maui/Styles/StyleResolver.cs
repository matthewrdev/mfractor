using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.StaticResources;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Styles
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IStyleResolver))]
    class StyleResolver : IStyleResolver
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IStaticResourceResolver> staticResourceResolver;
        public IStaticResourceResolver StaticResourceResolver => staticResourceResolver.Value;

        [ImportingConstructor]
        public StyleResolver(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                             Lazy<IStaticResourceResolver> staticResourceResolver)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.staticResourceResolver = staticResourceResolver;
        }

        public IEnumerable<IStyle> ResolveAvailableStyles(Project project, IXamlPlatform platform, string filePath)
        {
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database == null || !database.IsValid)
            {
                return Enumerable.Empty<IStyle>();
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return Enumerable.Empty<IStyle>();
            }

            var resources = StaticResourceResolver.GetAvailableResources(project, platform, filePath);
            if (resources == null || !resources.Any())
            {
                return Enumerable.Empty<IStyle>();
            }

            var staticResources = resources.GetStyleStaticResourceDefinitions();
            if (!staticResources.Any())
            {
                return Enumerable.Empty<IStyle>();
            }

            if (!staticResources.Any())
            {
                return Enumerable.Empty<IStyle>();
            }

            return CreateStyles(staticResources, project, platform);
        }

        public IEnumerable<IStyle> ResolveStylesByTargetType(Project project, IXamlPlatform platform, string filePath, INamedTypeSymbol targetType)
        {
            if (project == null || targetType == null)
            {
                return Enumerable.Empty<IStyle>();
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database == null || !database.IsValid)
            {
                return Enumerable.Empty<IStyle>();
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return Enumerable.Empty<IStyle>();
            }

            var resources = StaticResourceResolver.GetAvailableResources(project, platform, filePath);
            if (resources == null || !resources.Any())
            {
                return Enumerable.Empty<IStyle>();
            }

            var staticResources = resources.GetStyleStaticResourceDefinitions();
            if (!staticResources.Any())
            {
                return Enumerable.Empty<IStyle>();
            }

            var styleRepo = database.GetRepository<StyleDefinitionRepository>();

            var styles = new List<IStyle>();

            foreach (var staticResource in staticResources)
            {
                var definition = styleRepo.GetStyleDefinitionForStaticResource(staticResource);

                var styleTargetType = compilation.GetTypeByMetadataName(staticResource.TargetType);
                if (!SymbolHelper.DerivesFrom(styleTargetType, targetType))
                {
                    continue;
                }

                var style = CreateStyle(definition, project, platform);

                if (style != null)
                {
                    styles.Add(style);
                }
            }

            return styles;
        }

        public IStyle ResolveStyleByName(Project project, IXamlPlatform platform, string filePath, string styleName)
        {
            if (string.IsNullOrEmpty(styleName))
            {
                return default;
            }

            var staticResources = ResolveNamedStaticResources(project, platform, filePath, styleName);

            if (staticResources == null || !staticResources.Any())
            {
                return default;
            }

            var styles = CreateStyles(staticResources, project, platform);

            return styles.FirstOrDefault();
        }

        IEnumerable<StaticResourceDefinition> ResolveNamedStaticResources(Project project, IXamlPlatform platform,  string filePath, string styleName)
        {
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database == null || !database.IsValid)
            {
                return Enumerable.Empty<StaticResourceDefinition>();
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return Enumerable.Empty<StaticResourceDefinition>();
            }

            var resources = StaticResourceResolver.FindNamedStaticResources(project, platform, filePath, styleName);

            if (resources == null || !resources.Any())
            {
                return Enumerable.Empty<StaticResourceDefinition>();
            }

            var staticResources = resources.Find((project, resource) =>
            {
                var type = compilation.GetTypeByMetadataName(resource.SymbolMetaType);
                if (type == null)
                {
                    return false;
                }

                return SymbolHelper.DerivesFrom(type, platform.Style.MetaType);
            });

            return staticResources;
        }

        public IEnumerable<IStyle> CreateStyles(IEnumerable<StaticResourceResult> resources, Project project, IXamlPlatform platform)
        {
            return CreateStyles(resources.Select(r => r.Definition), project, platform);
        }

        public IEnumerable<IStyle> CreateStyles(IEnumerable<StaticResourceDefinition> resources, Project project, IXamlPlatform platform)
        {
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database == null || !database.IsValid)
            {
                return Enumerable.Empty<IStyle>();
            }

            var styleRepo = database.GetRepository<StyleDefinitionRepository>();

            var styles = new List<IStyle>();

            foreach (var resource in resources)
            {
                var definition = styleRepo.GetStyleDefinitionForStaticResource(resource);

                var style  = CreateStyle(definition, project, platform);

                if (style != null)
                {
                    styles.Add(style);
                }
            }

            return styles;
        }

        public IStyle CreateStyle(StyleDefinition styleDefinition, Project project, IXamlPlatform platform)
        {
            if (styleDefinition is null)
            {
                return default;
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database == null || !database.IsValid)
            {
                return default;
            }

            var styleRepo = database.GetRepository<StyleDefinitionRepository>();
            var setterRepo = database.GetRepository<StyleSetterRepository>();
            var projectFileRepo = database.GetRepository<ProjectFileRepository>();

            var properties = new Dictionary<string, List<IStyleProperty>>();

            AddProperties(styleDefinition, setterRepo, ref properties);

            var baseStyleName = styleDefinition.BaseStyleName;
            var projectFileKey = styleDefinition.ProjectFileKey;

            var inheritanceChain = new List<string>();

            while (!string.IsNullOrEmpty(baseStyleName))
            {
                var projectFile = projectFileRepo.Get(projectFileKey);

                var definition = ResolveNamedStaticResources(project, platform, projectFile.FilePath, baseStyleName).FirstOrDefault();

                if (definition != null)
                {
                    var baseStyle = styleRepo.GetStyleDefinitionForStaticResource(definition);

                    if (baseStyle != null)
                    {
                        AddProperties(baseStyle, setterRepo, ref properties);

                        baseStyleName = baseStyle.BaseStyleName;
                        projectFileKey = baseStyle.ProjectFileKey;
                    }
                    else
                    {
                        baseStyleName = null;
                    }
                }
                else
                {
                    baseStyleName = null;
                }
            }

            var style = new Style(styleDefinition.TargetType, styleDefinition.Name, styleDefinition.IsImplicitStyle, inheritanceChain, new StylePropertyCollection(properties));

            return style;
        }

        void AddProperties(StyleDefinition styleDefinition, StyleSetterRepository setterRepo, ref Dictionary<string, List<IStyleProperty>> properties)
        {
            var setters = setterRepo.GetPropertiesForStyle(styleDefinition);

            foreach (var setter in setters)
            {
                if (!properties.ContainsKey(setter.Property))
                {
                    properties[setter.Property] = new List<IStyleProperty>();
                }

                var priority = properties[setter.Property].Count;

                var property = new StyleProperty(setter.StyleDefinitionKey, setter.Property, new LiteralStylePropertyValue(setter.Value), priority);
                properties[setter.Property].Add(property);
            }
        }
    }
}