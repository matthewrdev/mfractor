using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Colors
{
    class MalformedHexadecimalColorValue : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1084";

        public override string Identifier => "com.mfractor.code.analysis.xaml.malformed_hexadecimal_color";

        public override string Name => "Malformed Hexadecimal Color Value";

        public override string Documentation => "Inspects hexadecimal color values and validates that they are in a supported format.";

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

            var value = syntax.Value.Value;

            if (!value.StartsWith("#", StringComparison.Ordinal))
            {
                return null;
            }

            if (ColorHelper.TryParseHexColor(value, out _, out _))
            {
                return null;
            }

            var formats = string.Empty;
            formats += "\n" + "#RRGGBB" + " - A color with 32 bit (00-FF) Red, Green and Blue channels.";
            formats += "\n" + "#AARRGGBB" + " - A color with 32 bit (00-FF) Alpha, Red, Green and Blue channels.";
            formats += "\n" + "#RGB" + " - A color with 16 bit (0-F) Red, Green and Blue channels.";
            formats += "\n" + "#ARGB" + " - A color with 16 bit (0-F) Alpha, Red, Green and Blue channels.";

            return CreateIssue($"The hexadecimal color '{syntax.Value.Value}' does not match any supported hex color formats.\n\nSupported Formats are:" + formats, syntax, syntax.Value.Span).AsList();
        }
    }
}
