using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Data;
using MFractor.Maui.Analysis.Thickness;
using MFractor.Maui.CodeGeneration.Thickness;
using MFractor.Maui.Data.Repositories;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Data;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Thickness
{
    class ReplaceThicknessValueWithStaticResource : FixCodeAction
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        public override Type TargetCodeAnalyser => typeof(ThicknessValueCanBeReplacedByStaticResource);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.replace_thickness_value_with_static_resource";

        public override string Name => "Replace Thickness Value With Static Resource";

        public override string Documentation => "When a thickness value matches a static resource thickness, this code fix replaces the value with the named thickness";

        [Import]
        public IThicknessUsageConsolidator ThicknessUsageConsolidator { get; set; }

        [ImportingConstructor]
        public ReplaceThicknessValueWithStaticResource(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ThicknessValueCanBeReplacedByStaticResourceBundle>();

            return bundle != null && bundle.MatchingThicknessDefinitions.Any();
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ThicknessValueCanBeReplacedByStaticResourceBundle>();

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            var repo = database.GetRepository<ThicknessUsageRepository>();

            var suggestions = new List<ICodeActionSuggestion>();

            for (var i = 0; i < bundle.MatchingThicknessDefinitions.Count; ++i)
            {
                var id = i + 1;
                var match = bundle.MatchingThicknessDefinitions[i];
                suggestions.Add(CreateSuggestion("Replace with " + match.Name, id));

                var count = repo.GetCountOfThicknessUsagesWithValue(match.FormattedValue);

                if (count > 1)
                {
                    suggestions.Add(CreateSuggestion($"Replace all usages of {syntax.Value.Value} with " + match.Name, id * -1));
                }
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ThicknessValueCanBeReplacedByStaticResourceBundle>();

            var actionId = suggestion.ActionId;
            var actionIndex = Math.Abs(actionId) - 1;

            var match = bundle.MatchingThicknessDefinitions[actionIndex];

            if (actionId < 0)
            {
                return ThicknessUsageConsolidator.Consolidate(context.Project, context.Platform, match.Name, match.FormattedValue, false);
            }

            return new ReplaceTextWorkUnit(document.FilePath, "{" + context.Platform.StaticResourceExtension.MarkupExpressionName + " " + match.Name + "}", syntax.Value.Span).AsList();
        }
    }
}
