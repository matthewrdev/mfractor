using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Thickness
{
    class InvalidThicknessAttributeValue : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects attribute properties that use the `Thickness` type and checks the value used can be translated to a thickness. For example, `Thickness` could accidentally be provided `0,5,05`, with the intention of it being `0,5,0,5`; the first example has three arguments while the second has four. This misuse would cause the app to crash when using inflated XAML or for XAMLC to fail.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.invalid_thickness_attribute_value";

        public override string Name => "Invalid Thickness Attribute Value";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1063";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ThicknessExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return Array.Empty<ICodeIssue>();
            }

            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (property == null)
            {
                return Array.Empty<ICodeIssue>();
            }

            var thicknessType = context.Compilation.GetTypeByMetadataName(context.Platform.Thickness.MetaType);
            if (!property.Type.Equals(thicknessType))
            {
                return Array.Empty<ICodeIssue>();
            }

            if (syntax.HasValue == false)
            {
                return Array.Empty<ICodeIssue>();
            }

            if (ThicknessHelper.ProcessThickness(syntax.Value.Value, out var thickness))
            {
                return Array.Empty<ICodeIssue>();
            }

            var formats = "uniformSize" + " - A thickness that represents a uniform thickness of size " + "uniformSize" + ".";
            formats += "\n" + "horizontalSize,verticalSize" + " - A thickness with a horizontal thickness of " + "horizontalSize" + " and a vertical thickness of " + "verticalSize" + ".";
            formats += "\n" + "left,top,right,bottom" + " - A thickness defined by " + "left" + ", " + "top" + ", " + "right" + ", and " + "bottom" + ".";

            return CreateIssue("An invalid number of arguments has been provided for this thickness value.\n\nFormats:\n" + formats, syntax, syntax.Value.Span).AsList();
        }
    }
}

