using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Thickness;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Utilities;
using MFractor.Ide.WorkUnits;
using MFractor.Work;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Xml;
using MFractor.Utilities;

namespace MFractor.Maui.CodeActions.Fix.Thickness
{
    class ViewThicknessesForConsolidation : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(ThicknessCanBeConsolidated);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.view_thicknesses_for_consolidation";

        public override string Name => "View Thicknesses For Consolidation";

        public override string Documentation => "Allows the user to view all thicknesses that can be consolidated in the find in files part of the IDE.";

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public ViewThicknessesForConsolidation(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"View all usages of {syntax.Value.Value}").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            ThicknessHelper.ProcessThickness(syntax.Value.Value, out var thickness);

            var formattedValue = ThicknessHelper.ToFormattedValueString(thickness);

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);

            var repo = database.GetRepository<ThicknessUsageRepository>();
            var projectFileRepo = database.GetRepository<ProjectFileRepository>();

            var usages = repo.GetThicknessUsagesWithValue(formattedValue);

            var results = usages.Select(usage =>
            {
                var file = projectFileRepo.GetProjectFileById(usage.ProjectFileKey);
                return new NavigateToFileSpanWorkUnit(usage.ValueSpan, file.FilePath);
            }).ToList();

            return new NavigateToFileSpansWorkUnit(results).AsList();
        }
    }
}
