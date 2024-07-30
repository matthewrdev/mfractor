using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.Styles
{
    class ReplaceStyleSetterPropertyWithAutocorrectionFix : FixCodeAction
    {
        public override string Documentation => "When a style setter tries to use a property that doesn't exist on the styles `TargetType` but the name closely matches an existing member on that type, this code fix will replace the incorrect value with a suggested correction.";

        public override Type TargetCodeAnalyser => typeof(Analysis.StylePropertySetterDoesNotExistAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.style_property_setter_does_not_exist";

        public override string Name => "Replace Style Setter Property With Autocorrection";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
		{
			var symbol = issue.GetAdditionalContent<ISymbol>();

            return symbol != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
		{
			var symbol = issue.GetAdditionalContent<ISymbol>();

            return CreateSuggestion($"Replace '{syntax.Value}' with '{symbol.Name}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
			var symbol = issue.GetAdditionalContent<ISymbol>();
            return new ReplaceTextWorkUnit(document.FilePath, symbol.Name, syntax.Value.Span).AsList();
        }
    }
}

