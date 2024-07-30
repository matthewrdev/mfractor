using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.EventHandlers
{
    class EmptyEventHandlerAssignment : XamlCodeAnalyser
    {
        public override string Documentation => "Checks that an event handler assignment is not empty as empty event handler assignments will cause a compilation error.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.empty_event_handler_assignment";

        public override string Name => "Empty Event Handler Assignment";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1012";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.EventHandlerExecutionFilter;

        protected override bool IsInterestedInXamlDocument(IParsedXamlDocument document,
                                                           IXamlFeatureContext context)
        {
            return document.CodeBehindSymbol != null;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var eventSymbol = context.XamlSemanticModel.GetSymbol(syntax) as IEventSymbol;
            if (eventSymbol == null || syntax.HasValue)
            {
                return null;
            }

            return CreateIssue($"This event handler has no callback assigned.", syntax, syntax.Span).AsList();
        }
    }
}
