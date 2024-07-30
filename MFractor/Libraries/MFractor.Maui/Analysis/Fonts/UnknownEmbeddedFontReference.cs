using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Analysis.Preprocessors;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.Fonts
{
    class UnknownEmbeddedFontReference : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects FontFamily attributes and validates that the referenced embedded font asset is defined.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.unknown_embedded_font_reference";

        public override string Name => "Unknown Embedded Font Reference";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1095";

        protected override bool IsInterestedInXamlDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return context.Platform.SupportsExportFontAttribute;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != "FontFamily"
                || !syntax.HasValue)
            {
                return null;
            }

            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
            {
                return null;
            }

             if (!TryGetPreprocessor<ExportedFontPreprocessor>(context, out var preprocessor))
            {
                return null;
            }

            var fonts = preprocessor.EmbeddedFonts;

            var value = syntax.Value.Value;

            var hasMatch = fonts.Any(f =>
            {
                if (f.HasAlias)
                {
                    return f.Alias == value;
                }

                return f.FontName == value;
            });

            if (hasMatch)
            {
                return null;
            }

            var message = $"The exported font '{syntax.Value}' is not defined within this project.";
            var nearest = SuggestionHelper.FindBestSuggestion(syntax.Value.Value, fonts.Select(f => f.LookupName));
            if (!string.IsNullOrEmpty(nearest))
            {
                message += $"\n\nDid you mean '{nearest}'?";
            }

            return CreateIssue(message, syntax, syntax.Value.Span, nearest).AsList();
        }
    }
}

