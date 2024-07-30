using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Colors;
using MFractor.Maui.CodeGeneration.Colors;
using MFractor.Maui.Configuration;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Colors
{
    class ConsolidateColors : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(ColorValueCanBeConsolidated);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.consolidate_colors";

        public override string Name => "Consolidate Colors";

        public override string Documentation => "When a specific color value is used multiple time throughout the application, this refactoring allows the user to extract the value into a static resource in the App.xaml and replace all duplications of that color with a static resource.";
        
        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IColorUsageConsolidator ColorUsageConsolidator { get; set; }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            return appXaml != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Consolidate all colors of {syntax.Value.Value} into a static resource.").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            ColorHelper.TryEvaluateColor(syntax.Value.Value, out var color);

            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            IReadOnlyList<IWorkUnit> onNameConfirmed(string name)
            {
                return ColorUsageConsolidator.Consolidate(context.Project, context.Platform, name, color, true);
            }

            return new TextInputWorkUnit("Consolidate Color",
                                              $"Enter the name of the new color resource for '{syntax.Value.Value}'",
                                              $"myColor",
                                              "Consolidate",
                                              "Cancel",
                                              onNameConfirmed).AsList();
        }
    }
}
