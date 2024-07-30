using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.Setter;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Setters
{
    class RemoveDuplicateSetter : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(StyleOrTriggerHasDuplicateSetters);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_fixes.xaml.remove_duplicate_setter";

        public override string Name => "Remove Duplicate Setter";

        public override string Documentation => "When a style or trigger defines multiple setters for the same property, this code fix can be used to remove the duplicate setter";

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Remove duplicate setter").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new DeleteXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                Syntaxes = new List<XmlSyntax>() { syntax }
            }.AsList();
        }
    }
}