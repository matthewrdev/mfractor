using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.EventHandlers
{
    class EventHandlerSignatureMatchesAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Checks that the signature for the event callback in a code behind class matches the expected signature for the property it is assigned to.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.event_handler_signature_matches";

        public override string Name => "Event Handler Signature Mismatch";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1014";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.EventHandlerExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.MetaData == null
                || !syntax.HasValue)
            {
                return null;
            }

            if (document.CodeBehindSymbol == null)
            {
                return null;
            }

            var eventSymbol = context.XamlSemanticModel.GetSymbol(syntax) as IEventSymbol;
            if (eventSymbol == null)
            {
                return null;
            }

            var methodSymbol = SymbolHelper.FindMemberSymbolByName<IMethodSymbol>(document.CodeBehindSymbol, syntax.Value.Value);

            if (methodSymbol == null)
            {
                return null;
            }

            // Check if it's an event handler or delegate
            if (eventSymbol.Type.Name == "EventHandler")
            {
                if (methodSymbol.Parameters.Length != 2)
                {
                    return CreateIssue($"{methodSymbol.Name} has {methodSymbol.Parameters.Length} parameters; an event callback should have 2 arguments.", syntax, syntax.Value.Span).AsList();
                }
            }
            else
            {
                return null; // TODO: Check the signatures on the event delegate.
            }

            var namedEventType = eventSymbol.Type as INamedTypeSymbol;

            var eventTypeArg = namedEventType.TypeArguments.Length > 0 ? namedEventType.TypeArguments.First().ToString() : "System.EventArgs";

            if (string.IsNullOrEmpty(eventTypeArg))
            {
                return null;
            }

            var firstArgType = methodSymbol.Parameters[0].Type;
            var secondArgType = methodSymbol.Parameters[1].Type;

            if (SymbolHelper.DerivesFrom(secondArgType, eventTypeArg))
            {
                return null;
            }

            return CreateIssue($"Event Argument Mismatch: The event handler '{eventSymbol.Name}' will send a {eventTypeArg} but {methodSymbol.Name}'s event args is a {secondArgType.Name}.", syntax, syntax.Value.Span).AsList();
        }
    }
}
