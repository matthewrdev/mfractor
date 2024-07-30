//using System.Collections.Generic;
//using System.Linq;
//using MFractor.Code.Analysis;
//using MFractor.Maui.Syntax.Expressions;
//using MFractor.Utilities;
//using MFractor.Xml;
//using Microsoft.CodeAnalysis;

//namespace MFractor.Maui.Analysis.Colors
//{
//    class ColorValueCloselyMatchesNamedColor : XamlCodeAnalyser
//    {
//        public override CodeIssueCategory Category => CodeIssueCategory.Improvement;

//        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

//        public override string DiagnosticId => "MF1089";

//        public override string Identifier => "com.mfractor.code.analysis.xaml.color_value_closely_matches_named_color";

//        public override string Name => "Color Value Closely Matches Named Color";

//        public override string Documentation => "Inspects color values and checks if they closely match the color value defined by a named system color.";

//        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
//        {
//            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
//            if (property == null
//                || !syntax.HasValue
//                || !FormsSymbolHelper.IsColor(property.Type))
//            {
//                return Array.Empty<ICodeIssue>();
//            }

//            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
//            {
//                return Array.Empty<ICodeIssue>();
//            }

//            if (!ColorHelper.TryParseHexColor(syntax.Value.Value, out var color, out var hasAlpha))
//            {
//                return Array.Empty<ICodeIssue>();
//            }

//            var colors = ColorHelper.GetAllSystemDrawingColors();

//            var matches = colors.Where(cr => ColorHelper.ColorsAreClose(cr.Value, color)).ToList();

//            if (!matches.Any())
//            {
//                return default;
//            }

//            var message = $"This color, {syntax.Value.Value}, closely matches multiple named colors color resources.";
//            if (matches.Count == 1)
//            {
//                var firstMatch = matches.First();
//                message = $"This color, {syntax.Value.Value}, closely matches the color '{firstMatch.Key}'.";
//            }

//            message += "\n\nWould you like to replace this color value with a named color instead?";

//            return CreateIssue(message, syntax, syntax.Value.Span, new ColorValueCanBeReplacedWithStaticResourceBundle(matches)).AsList();
//        }
//    }
//}
