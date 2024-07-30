using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.Grid
{
    class GridRowColumnSetterIsNotANumber : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.grid_row_column_setter_is_not_a_number";

        public override string Name => "Grid Row/Column Setter Is Not A Number";

        public override string Documentation => "This code analyser inspects usages of the `Grid.Column` attribute and validates that the element is inside a `Grid`.";

        public override string DiagnosticId => "MF1027";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return base.Analyse(syntax, document, context);
        }
    }
}
