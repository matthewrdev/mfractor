using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.Fonts
{
    class InvalidNamedFontSizeAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects FontSize attributes and validates that the named size provided to it is a valid font size name (Micro, Small, Medium, Large).";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.invalid_named_font_size";

        public override string Name => "Invalid Named Font Size";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1020";

        public static readonly string[] FontSizes = {
            "Default",
            "Micro",
            "Small",
            "Medium",
            "Large",
            "Body",
            "Header",
            "Title",
            "Subtitle",
            "Caption",
        };

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != "FontSize"
                || !syntax.HasValue)
            {
                return null;
            }

            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
            {
                return null;
            }

            if (double.TryParse(syntax.Value.Value, out _))
            {
                return null;
            }

            if (FontSizes.Contains(syntax.Value.Value))
            {
                return null;
            }

            var message = $"The value '{syntax.Value}' is not a valid font size (a double or named size value expected).";
            var nearest = SuggestionHelper.FindBestSuggestion(syntax.Value.Value, FontSizes);
            if (string.IsNullOrEmpty(nearest))
            {
                message += " Valid named font sizes are:\n - " + string.Join("\n - ", FontSizes);
            } 
            else
            {
                message += $"\n\nDid you mean '{nearest}'?";
            }

            return CreateIssue(message, syntax, syntax.Value.Span, nearest).AsList();
        }
    }
}

