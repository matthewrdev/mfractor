using System.Collections.Generic;
using System.Linq;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.Documents;
using MFractor.Maui.Styles;
using MFractor.Maui.XamlPlatforms;
using MFractor.Workspace.Data;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// A <see cref="ICodeAnalysisPreprocessor"/> that caches styles.
    /// </summary>
    public class StyleAnalysisPreprocessor : ICodeAnalysisPreprocessor
    {
        readonly IStyleResolver styleResolver;
        readonly IResourcesDatabaseEngine resourcesDatabaseEngine;

        public StyleAnalysisPreprocessor(IStyleResolver styleResolver,
                                         IResourcesDatabaseEngine resourcesDatabaseEngine)
        {
            this.styleResolver = styleResolver;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        readonly Dictionary<string, IStyle> stylesByName = new Dictionary<string, IStyle>();
        public IReadOnlyDictionary<string, IStyle> StylesByName => stylesByName;

        readonly Dictionary<string, List<IStyle>> stylesByTargetType = new Dictionary<string, List<IStyle>>();
        public IReadOnlyDictionary<string, IReadOnlyList<IStyle>> StylesByTargetType => stylesByTargetType.ToDictionary(kp => kp.Key, kp => (IReadOnlyList<IStyle>)kp.Value);

        public bool IsValid { get; private set; }

        public bool Preprocess(IParsedXmlDocument document, IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            var xamlDocument = document as IParsedXamlDocument;

            if (xamlContext == null || xamlDocument == null)
            {
                return false;
            }

            var database = resourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            if (database == null || !database.IsValid)
            {
                return false;
            }

            //availableStyles = new Lazy<IReadOnlyList<IStyle>>( () =>
            //{
            //    using (Profiler.Profile("ResolveAvailableStyles"))
            //    {
            //        var result = styleResolver.ResolveAvailableStyles(context.Project, xamlContext.Platform, document.FilePath);

            //        if (result == null)
            //        {
            //            return new List<IStyle>();
            //        }

            //        return result.ToList();
            //    }
            //});

            IsValid = true;

            return true;
        }

        public IStyle GetNamedStyle(string name, string filePath, Project project, IXamlPlatform xamlPlatform)
        {
            if (string.IsNullOrEmpty(name)
                || string.IsNullOrEmpty(filePath)
                || project == null
                || xamlPlatform is null)
            {
                return default;
            }

            if (StylesByName.ContainsKey(name))
            {
                return StylesByName[name];
            }

            var style = this.styleResolver.ResolveStyleByName(project, xamlPlatform, filePath, name);
            stylesByName[name] = style;

            return style;
        }

        public IReadOnlyList<IStyle> GetStylesForTargetType(INamedTypeSymbol targetType, string filePath, Project project, IXamlPlatform xamlPlatform)
        {
            if (targetType == null
                || string.IsNullOrEmpty(filePath)
                || project == null
                || !project.TryGetCompilation(out var compilation)
                || xamlPlatform is null)
            {
                return default;
            }

            var key = targetType.ToString();

            if (StylesByTargetType.ContainsKey(key))
            {
                return StylesByTargetType[key];
            }

            var styles = this.styleResolver.ResolveStylesByTargetType(project, xamlPlatform, filePath, targetType);

            if (styles != null && styles.Any())
            {
                foreach (var style in styles)
                {
                    if (!StylesByTargetType.ContainsKey(style.TargetType))
                    {
                        stylesByTargetType[style.TargetType] = new List<IStyle>();
                    }

                    stylesByTargetType[style.TargetType].Add(style);
                }
            }

            StylesByTargetType.TryGetValue(key, out var result);

            return result;
        }
    }
}
