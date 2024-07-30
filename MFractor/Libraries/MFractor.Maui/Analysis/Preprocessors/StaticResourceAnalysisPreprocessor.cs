using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Data;
using MFractor.Code.Documents;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Code;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// A <see cref="ICodeAnalysisPreprocessor"/> implementation that caches the <see cref="StaticResourceDefinition"/>'s available to the current document.
    /// </summary>
    public class StaticResourceAnalysisPreprocessor : ICodeAnalysisPreprocessor
    {
        public StaticResourceAnalysisPreprocessor(IStaticResourceResolver staticResourceResolver,
                                                  IResourcesDatabaseEngine resourcesDatabaseEngine)
        {
            this.staticResourceResolver = staticResourceResolver;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        readonly IStaticResourceResolver staticResourceResolver;
        readonly IResourcesDatabaseEngine resourcesDatabaseEngine;

        Lazy<IStaticResourceCollection> staticResourceCollection;
        public IStaticResourceCollection StaticResourceCollection => staticResourceCollection.Value;

        Lazy<IReadOnlyList<StaticResourceDefinition>> allAvailableStaticResources;
        public IReadOnlyList<StaticResourceDefinition> AllAvailableStaticResources => allAvailableStaticResources.Value;

        Lazy<IReadOnlyList<string>> allAvailableStaticResourceNames;
        public IReadOnlyList<string> AllAvailableStaticResourceNames => allAvailableStaticResourceNames.Value;

        Lazy<Dictionary<string, IReadOnlyList<StaticResourceDefinition>>> namedStaticResources;
        public IReadOnlyDictionary<string, IReadOnlyList<StaticResourceDefinition>> NamedStaticResources => namedStaticResources.Value;

        public bool IsValid { get; private set; } = false;

        public bool Preprocess(IParsedXmlDocument document, IFeatureContext context)
        {
            // Initialise lazy's with a default.
            allAvailableStaticResources = new Lazy<IReadOnlyList<StaticResourceDefinition>>();
            namedStaticResources = new Lazy<Dictionary<string, IReadOnlyList<StaticResourceDefinition>>>();

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

            var project = xamlContext.Project;
            var filePath = xamlDocument.FilePath;

            IsValid = true;

            staticResourceCollection = new Lazy<IStaticResourceCollection>(() =>
            {
                return staticResourceResolver.GetAvailableResources(project, xamlContext.Platform, filePath);
            });

            allAvailableStaticResources = new Lazy<IReadOnlyList<StaticResourceDefinition>>(() =>
           {
               if (StaticResourceCollection is null)
               {
                   return new List<StaticResourceDefinition>();
               }

               return StaticResourceCollection.Find((p, r) => true);
           });

            allAvailableStaticResourceNames = new Lazy<IReadOnlyList<string>>(() =>
           {
               return NamedStaticResources.Select(n => n.Key).ToList();
           });

            namedStaticResources = new Lazy<Dictionary<string, IReadOnlyList<StaticResourceDefinition>>>(() =>
           {
               var result = new Dictionary<string, IReadOnlyList<StaticResourceDefinition>>();

               var count = AllAvailableStaticResources.Count;

               foreach (var resource in AllAvailableStaticResources.Where(r => r.IsExplicitResource)
                                                                   .GroupBy(r => r.Name))
               {
                   var name = resource.Key;
                   var values = resource.ToList();

                   result[name] = values;
               }

               return result;
           });

            return true;
        }

        public IEnumerable<StaticResourceDefinition> FindNamedStaticResources(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return Enumerable.Empty<StaticResourceDefinition>();
            }

            if (!NamedStaticResources.ContainsKey(resourceName))
            {
                return Enumerable.Empty<StaticResourceDefinition>();
            }

            return NamedStaticResources[resourceName];
        }
    }
}
