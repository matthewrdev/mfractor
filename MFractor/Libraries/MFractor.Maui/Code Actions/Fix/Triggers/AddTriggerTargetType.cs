using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Triggers;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Triggers
{
    class AddTriggerTargetType : FixCodeAction
    {
        public override string Documentation => "When a `DataTrigger` or `MultiTrigger` does not have the target type attribute, this code fix adds it and sets the target type based on the triggers parent.";

        public override Type TargetCodeAnalyser => typeof(TriggerIsMissingTargetType);

        public override string Identifier => "com.mfractor.code_fixes.xaml.add_trigger_target_type";

        public override string Name => "Add Trigger Target Type";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var outerParent = syntax.Parent.Parent?.Parent;

            return CreateSuggestion($"Add '{outerParent.Name.FullName}' as the target type for this trigger", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var outerParent = syntax.Parent?.Parent;
            var insertion = $" TargetType=\"{outerParent.Name.FullName}\"";

            return new InsertTextWorkUnit(insertion, syntax.NameSpan.End, document.FilePath).AsList();
        }
    }
}
