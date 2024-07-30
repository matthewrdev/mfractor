using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis
{
    class ColumnSpanIsZero : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.columnspan_is_zero";

        public override string Name => "ColumnSpan Is Zero";

        public override string Documentation => "This code analyser inspects usages of the `Grid.ColumnSpan` attribute and validates that the span provided is a non-zero value.";

        public override string DiagnosticId => "MF1026";

        const string columnSpanPropertyName = "Grid.ColumnSpan";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != columnSpanPropertyName
                && syntax.HasValue)
            {
                return null;
            }

            if (!int.TryParse(syntax.Value.Value, out var span))
            {
                return null;
            }

            if (span > 0)
            {
                return null;
            }

            return CreateIssue($"A ColumnSpan value cannot be zero.", syntax, syntax.Span).AsList();
        }
    }
}
