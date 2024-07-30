using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Sliders
{
    /// <summary>
    /// See: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/slider#precautions
    /// </summary>
    class SliderMinimumIsSetBeforeMaximum : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.slider_minimum_set_before_maximum";

        public override string Name => "Slider Minimum Set Before Maximum";

        public override string Documentation => "Inspects slider elements and verifies that the user sets the maximum before the minimum";

        public override string DiagnosticId => "MF1088";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var parent = syntax.Parent;

            var typeSymbol = context.XamlSemanticModel.GetSymbol(parent) as INamedTypeSymbol;
            if (typeSymbol == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(typeSymbol, context.Platform.Slider.MetaType))
            {
                return null;
            }

            var isMinMax = syntax.Name.FullName == "Minimum" || syntax.Name.FullName == "Maximum";
            if (!isMinMax)
            {
                return null;
            }

            var minimum = parent.GetAttributeByName("Minimum");
            var maximum = parent.GetAttributeByName("Maximum");

            if (maximum == null || minimum == null)
            {
                return null;
            }

            var minLocation = parent.Attributes.IndexOf(minimum);
            var maxLocation = parent.Attributes.IndexOf(maximum);

            if (maxLocation < minLocation)
            {
                return null;
            }

            return CreateIssue("The Slider minimum value is set before the maximum. This will cause the minimum to be evaluated first and may cause a runtime crash.", syntax, syntax.Span).AsList();
        }
    }
}
