using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Triggers;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.Triggers
{
    class FixTriggerTargetType : FixCodeAction
    {
        public override string Documentation => "When the target type of the trigger does not match the outer element, this code fix correct the targetted type.";

        public override Type TargetCodeAnalyser => typeof(TriggerTargetTypeDoesNotMatchParent);

        public override string Identifier => "com.mfractor.code_fixes.xaml.fix_trigger_target_type";

        public override string Name => "Fix Trigger Target Type";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var outerParent = syntax.Parent.Parent?.Parent;
            var targetType = context.XamlSemanticModel.GetSymbol(outerParent) as INamedTypeSymbol;

            return targetType != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var outerParent = syntax.Parent.Parent?.Parent;

            return CreateSuggestion($"Change to {outerParent.Name.FullName}", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var outerParent = syntax.Parent.Parent?.Parent;
            var targetType = context.XamlSemanticModel.GetSymbol(outerParent) as INamedTypeSymbol;

            return new ReplaceTextWorkUnit(document.FilePath, outerParent.Name.FullName, syntax.Value.Span).AsList();
        }
    }
}
