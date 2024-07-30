//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using MFractor.Code.Analysis;
//using MFractor.Maui.Syntax.Expressions;
//using MFractor.Utilities;
//using MFractor.Xml;
//using Microsoft.CodeAnalysis;

//namespace MFractor.Maui.Analysis.Colors
//{
//    class DuplicatedColorValuesInDocument : XamlCodeAnalyser
//    {
//        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

//        public override string DiagnosticId => "MF1087";

//        public override string Identifier => "com.mfractor.code.analysis.xaml.duplicated_color_values_in_document";

//        public override string Name => "Duplicated Hex Color Values In Document";

//        public override string Documentation => "Inspects all color values inside a XAML document and detects any duplicated colors that could be refactored into a static resource.";

//        public override IssueClassification Classification => IssueClassification.Improvement;

//        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
//        {
//            if (!syntax.IsRoot)
//            {
//                return Array.Empty<ICodeIssue>();
//            }

//            var colors = FindColors(syntax, context);

//            if (!colors.Any())
//            {
//                return Array.Empty<ICodeIssue>();
//            }

//            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
//            {
//                return default;
//            }

//            Color color;
//            try
//            {
//                color = ColorTranslator.FromHtml(syntax.Value.Value);
//            }
//            catch
//            {
//                return default;
//            }

//            if (!TryGetPreprocessor<StaticResourceAnalysisPreprocessor>(context, out var preprocessor))
//            {
//                return default;
//            }

//            var colorResources = preprocessor.AvailableColorDefinitions;

//            var matches = colorResources.Where(cr => ColorsAreClose(cr.Color, color)).ToList();

//            if (!matches.Any())
//            {
//                return default;
//            }

//            var message = $"This color, {syntax.Value.Value}, closely matches multiple available color resources.";
//            if (matches.Count == 1)
//            {
//                var firstMatch = matches.First();
//                message = $"This color, {syntax.Value.Value}, closely matches the color defined by the resource '{firstMatch.Name}'.";
//            }

//            message += "\n\nWould you like to replace this color value with a static resource lookup instead?";

//            return CreateIssue(message, syntax, syntax.Value.Span, new ColorValueCanBeReplacedWithStaticResourceBundle(matches)).AsList();
//        }

//        private Dictionary<Color, List<XmlAttribute>> FindColors(XmlNode syntax, IXamlFeatureContext context)
//        {
//            var result = new Dictionary<Color, List<XmlAttribute>>();

//            if (syntax.HasChildren)
//            {
//                foreach (var child in syntax.Children)
//                {
//                    var inner = FindColors
//                }

//            }

//            if (syntax.HasAttributes)
//            {

//            }

//            return resultl
//        }
//    }
//}
