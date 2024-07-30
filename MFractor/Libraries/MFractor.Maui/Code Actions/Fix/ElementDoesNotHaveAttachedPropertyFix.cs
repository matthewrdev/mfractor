using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions
{
    class ElementDoesNotHaveAttachedPropertyFix : FixCodeAction
	{
        public override string Documentation => "Replaces an incorrect attached property with an auto-corrected value.";

        public override Type TargetCodeAnalyser => typeof(Analysis.ElementDoesNotHaveAttachedPropertyAnalysis);
         
        public override string Identifier => "com.mfractor.code_fixes.xaml.no_attached_property";

        public override string Name => "Replace with correct attached property name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<ISymbol>();

            return symbol != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<ISymbol>();

            var typeName = syntax.Name.LocalName.Split('.').First();
			var newName = typeName + "." + symbol.Name.Substring(0, symbol.Name.LastIndexOf("Property", System.StringComparison.Ordinal));

			return CreateSuggestion("Replace with '" + newName + "'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<ISymbol>();

            var typeName = syntax.Name.LocalName.Split('.').First();
			var newName = typeName + "." + symbol.Name.Substring(0, symbol.Name.LastIndexOf("Property"));

            return new ReplaceTextWorkUnit(document.FilePath, newName, syntax.NameSpan).AsList();
        }
	}
}

