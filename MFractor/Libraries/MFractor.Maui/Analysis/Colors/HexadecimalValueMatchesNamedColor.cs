using System;
using System.Collections.Generic;
using System.Drawing;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Colors
{
    class HexadecimalValueMatchesNamedColor : XamlCodeAnalyser
    {
        public IReadOnlyDictionary<string, Color> Colors { get; } = ColorHelper.GetAllSystemDrawingColors();

        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1076";

        public override string Identifier => "com.mfractor.code.analysis.xaml.hexadecimal_value_matches_named_color";

        public override string Name => "Hexadecimal Value Matches Named Color";

        public override string Documentation => "Inspects HexaDecimal color values and matches them against the named color constants.";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ColorExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (property == null
                || !syntax.HasValue
                || !FormsSymbolHelper.IsColor(property.Type, context.Platform))
            {
                return null;
            }

            if (!ColorHelper.GetMatchingColor(syntax.Value.Value, out var match))
            {
                return null;
            }

            return CreateIssue($"The color '{syntax.Value.Value}' can be replaced with the named color '{match.Name}'", syntax, syntax.Value.Span).AsList();
        }
    }
}
