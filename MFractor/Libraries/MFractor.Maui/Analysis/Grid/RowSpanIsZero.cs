using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.Grid
{
    class RowSpanIsZero : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.rowspan_is_zero";

        public override string Name => "RowSpan Is Zero";

        public override string Documentation => "This code analyser inspects usages of the `Grid.RowSpan` attribute and validates that the span provided is a non-zero value.";

        public override string DiagnosticId => "MF1034";

        const string rowSpanPropertyName = "Grid.RowSpan";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != rowSpanPropertyName
                && syntax.HasValue)
            {
                return null;
            }

            var span = 0;
            if (!int.TryParse(syntax.Value.Value, out span))
            {
                return null;
            }

            if (span > 0)
            {
                return null;
            }

            return CreateIssue($"A RowSpan value cannot be zero.", syntax, syntax.Span).AsList();
        }
    }
}
