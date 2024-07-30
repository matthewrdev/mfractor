using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.Configuration;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Thickness
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IThicknessUsageConsolidator))]
    class ThicknessUsageConsolidator : XamlCodeGenerator, IThicknessUsageConsolidator
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        public override string Identifier => "com.mfractor.code_gen.xaml.thickness_usage_consolidator";

        public override string Name => "Thickness Usage Consolidation";

        public override string Documentation => "A code generator that finds all usages of a particular thickness value and replaces them with a static resource lookup";

        [ImportingConstructor]
        public ThicknessUsageConsolidator(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, IReadOnlyList<double> thickness, bool shouldCreateResourceDefinition)
        {
            var formattedValue = ThicknessHelper.ToFormattedValueString(thickness);

            return Consolidate(project, platform, resourceName, formattedValue, shouldCreateResourceDefinition);
        }

        public IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, double left, double right, double top, double bottom, bool shouldCreateResourceDefinition)
        {
            var formattedValue = ThicknessHelper.ToFormattedValueString(left, right, top, bottom);

            return Consolidate(project, platform, resourceName, formattedValue, shouldCreateResourceDefinition);
        }

        public IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, string thicknessFormattedValue, bool shouldCreateResourceDefinition)
        {
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            var repo = database.GetRepository<ThicknessUsageRepository>();
            var projectFileRepo = database.GetRepository<ProjectFileRepository>();

            var usages = repo.GetThicknessUsagesWithValue(thicknessFormattedValue);

            var workUnits = new List<IWorkUnit>();

            if (shouldCreateResourceDefinition)
            {
                var appXaml = AppXamlConfiguration.ResolveAppXamlFile(project, platform);

                var entry = new XmlNode(platform.Thickness.Name);
                entry.AddAttribute("x:Key", resourceName);
                entry.Value = thicknessFormattedValue;

                workUnits.AddRange(InsertResourceEntryGenerator.Generate(appXaml, entry));
            }

            var resourceExpression = "{ " + platform.StaticResourceExtension.MarkupExpressionName + " " + resourceName + "}";

            var projectFileKeys = new HashSet<int>();

            foreach (var usage in usages)
            {
                var file = projectFileRepo.GetProjectFileFor(usage);

                if (file != null)
                {
                    projectFileKeys.Add(file.PrimaryKey);
                    workUnits.Add(new ReplaceTextWorkUnit(file.FilePath, resourceExpression, usage.ValueSpan));
                }
            }

            workUnits.Add(new StatusBarMessageWorkUnit()
            {
                IsPostProcessed = true,
                Message = $"The thickness {thicknessFormattedValue} has been replaced with {resourceName} in {projectFileKeys.Count} files."
            });

            return workUnits;
        }
    }
}