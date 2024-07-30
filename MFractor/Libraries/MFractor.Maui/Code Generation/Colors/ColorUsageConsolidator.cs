using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.Configuration;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Colors
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IColorUsageConsolidator))]
    class ColorUsageConsolidator : XamlCodeGenerator, IColorUsageConsolidator
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        public override string Identifier => "com.mfractor.code_gen.xaml.color_usage_consolidator";

        public override string Name => "Color Usage Consolidation";

        public override string Documentation => "A code generator that finds all usages of a particular color value and replaces them with a static resource lookup";

        [ImportingConstructor]
        public ColorUsageConsolidator(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public IReadOnlyList<IWorkUnit> Consolidate(Project project, IXamlPlatform platform, string resourceName, Color color, bool shouldCreateResourceDefinition)
        {
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            var repo = database.GetRepository<ColorUsageRepository>();
            var projectFileRepo = database.GetRepository<ProjectFileRepository>();

            var usages = repo.GetHexadecimalColorUsagesWithValue(color);

            var workUnits = new List<IWorkUnit>();

            var hexString = ColorHelper.GetHexString(color, true);

            if (shouldCreateResourceDefinition)
            {
                var appXaml = AppXamlConfiguration.ResolveAppXamlFile(project, platform);

                var entry = new XmlNode("Color");
                entry.AddAttribute("x:Key", resourceName);
                entry.Value = hexString;

                workUnits.AddRange(InsertResourceEntryGenerator.Generate(appXaml, entry));
            }

            var resourceExpression = "{" + platform.StaticResourceExtension.MarkupExpressionName + " " + resourceName + "}";

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
                Message = $"The color {hexString} has been replaced with {resourceName} in {projectFileKeys.Count} files."
            });

            return workUnits;
        }
    }
}