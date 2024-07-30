using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Thickness
{
    class ThicknessValueCanBeSimplified : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1078";

        public override string Identifier => "com.mfractor.code.analysis.xaml.thickness_value_can_be_simplified";

        public override string Name => "Thickness Value Can Be Simplified";

        public override string Documentation => "Inspect's thickness attribute values and verifies if the values can be simplified. For example, a thickness value of `20,0,20,0` could be simplified to `20,0`.";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ThicknessExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return null;
            }

            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (property == null)
            {
                return null;
            }

            var thicknessType = context.Compilation.GetTypeByMetadataName(context.Platform.Thickness.MetaType);
            if (property.Type != thicknessType)
            {
                return null;
            }
            if (syntax.HasValue == false)
            {
                return null;
            }

            var values = syntax.Value.Value.Split(',')
                                     .Select(v => v.Trim())
                                     .Where(v => double.TryParse(v, out var __))
                                     .Select(v => double.Parse(v))
                                     .ToList();

            // Verify that it is a valid value to simplify.
            if (!(values.Count == 2 // EG: Padding="1,0" -> Left and right dimensions at 1, top and bottom at 0
               || values.Count == 4)) // EG: Padding="1,0,2,3" -> Left at 1, top at 0, right at 2, bottom at 3

            {
                return null;
            }

            var simplification = string.Empty;
            if (values.Count == 2)
            {
                if (Math.Abs(values[0] - values[1]) < double.Epsilon)
                {
                    simplification = values[0].ToString();
                }
            }
            else if (values.Count == 4)
            {
                if (values.Distinct().Count() == 1)
                {
                    simplification = values[0].ToString();
                }
                else if (Math.Abs(values[0] - values[2]) < double.Epsilon
                    && Math.Abs(values[1] - values[3]) < double.Epsilon)
                {
                    simplification = values[0].ToString() + "," + values[1].ToString();
                }
            }

            if (string.IsNullOrEmpty(simplification))
            {
                return null;
            }

            return CreateIssue($"This thickness value can be simplified to '{simplification}'", syntax, syntax.Value.Span, new ThicknessValueCanBeSimplifiedBundle(simplification)).AsList();
        }
    }
}
