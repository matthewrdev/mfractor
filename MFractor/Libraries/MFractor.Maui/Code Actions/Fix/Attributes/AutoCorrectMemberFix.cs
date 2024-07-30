using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Attributes
{
    class AutoCorrectMemberFix : FixCodeAction
	{
        public override Type TargetCodeAnalyser => typeof(PropertySetterAttributeDoesNotExistInParentAnalysis);

        public override string Documentation => "Looks for members on a C# class that are named closely to an unresolved xml attribute and then suggest near matches.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.autocorrect_member";

        public override string Name => "Correct Member Name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<PropertySetterAttributeDoesNotExistInParentBundle>();

            return bundle != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<PropertySetterAttributeDoesNotExistInParentBundle>();

            return CreateSuggestion("Replace with " + bundle.Suggestion).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<PropertySetterAttributeDoesNotExistInParentBundle>();

            return new ReplaceTextWorkUnit(document.FilePath, bundle.Suggestion, syntax.NameSpan).AsList();
        }
	}
}

