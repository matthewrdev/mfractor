using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Thickness;
using MFractor.Maui.CodeGeneration.Thickness;
using MFractor.Maui.Configuration;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Thickness
{
    class ConsolidateThicknesses : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(ThicknessCanBeConsolidated);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.consolidate_thicknesses";

        public override string Name => "Consolidate Thickness";

        public override string Documentation => "When a specific thickness value is used multiple time throughout the application, this refactoring allows the user to extract the value into a static resource in the App.xaml and replace all duplications of that thickness with a static resource.";
        
        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IThicknessUsageConsolidator ThicknessUsageConsolidator { get; set; }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            return appXaml != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Consolidate all thicknesses of {syntax.Value.Value} into a static resource.").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            ThicknessHelper.ProcessThickness(syntax.Value.Value, out var thickness);

            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            IReadOnlyList<IWorkUnit> onNameConfirmed(string name)
            {
                return ThicknessUsageConsolidator.Consolidate(context.Project, context.Platform, name, thickness, true);
            }

            return new TextInputWorkUnit("Consolidate Thickness",
                                              $"Enter the name of the new thickness resource for '{syntax.Value.Value}'",
                                              $"myThickness",
                                              "Consolidate",
                                              "Cancel",
                                              onNameConfirmed).AsList();
        }
    }
}
