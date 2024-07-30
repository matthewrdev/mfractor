using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.EventHandlers
{
    class EventHandlerDoesNotExistInCodeBehindAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Checks that an event callback referenced referenced by an attribute value exists in the code behind class.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.event_handler_does_not_exist_in_code_behind_classs";

        public override string Name => "Event Handler Exists In Code Behind Class";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1013";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.EventHandlerExecutionFilter;

        protected override bool IsInterestedInXamlDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return document.CodeBehindSymbol != null;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			if (syntax.MetaData == null
                || !syntax.HasValue)
			{
				return null;
			}

            var eventSymbol = context.XamlSemanticModel.GetSymbol(syntax) as IEventSymbol;
            if (eventSymbol == null)
			{
				return null;
			}

            ISymbol symbol = SymbolHelper.FindMemberSymbolByName<IEventSymbol>(document.CodeBehindSymbol, syntax.Value.Value);
			if (symbol == null)
			{
                symbol = SymbolHelper.FindMemberSymbolByName<IMethodSymbol>(document.CodeBehindSymbol, syntax.Value.Value);
			}

			if (symbol != null)
			{
				return null;
			}

            var callbacks = SymbolHelper.GetAllMemberSymbols<IMethodSymbol>(document.CodeBehindSymbol)
                                        .Where(p => p.Parameters.Length == 2)
                                        .Where(p => SymbolHelper.DerivesFrom(p.Parameters[1].Type, "System.EventArgs"));

            var message = $"A method named '{syntax.Value}' does not exist in the code behind class.";

            var suggestion = SuggestionHelper.FindBestSuggestion(syntax.Value.Value, callbacks.Select(c => c.Name));

            if (!string.IsNullOrEmpty(suggestion))
            {
                message += "\n\nDid you mean " + suggestion + "?";
            }

            var bundle = new EventHandlerDoesNotExistInCodeBehindBundle(eventSymbol, callbacks);

            return CreateIssue(message, syntax, syntax.Value.Span, bundle).AsList();
		}
	}
}

