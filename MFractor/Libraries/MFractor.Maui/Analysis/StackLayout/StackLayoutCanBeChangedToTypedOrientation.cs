using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.StackLayout
{
    class StackLayoutCanBeChangedToTypedOrientation : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.stack_layout_to_typed_orientation";

        public override string Name => "StackLayout Can Be Changed To VerticalStackLayout or HorizontalStackLayout";

        public override string Documentation => "This code analyser inspects usages of `StackLayout` and, depending on its orientation, suggests changing to VerticalStackLayout or HorizontalStackLayout";

        public override string DiagnosticId => "MF1104";

        protected override bool IsInterestedInXamlDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return context.Platform.SupportsTypedOrientationStackLayouts;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (SymbolHelper.DerivesFrom(symbol, context.Platform.StackLayout.MetaType))
            {
                return null;
            }

            var orientation = syntax.GetAttributeByName(context.Platform.OrientationProperty);
            if (orientation == null || !orientation.HasValue)
            {
                return null;
            }

            var orientationValue = orientation.Value.Value;
            if (orientationValue == context.Platform.StackLayoutOrientation_Vertical)
            {
                return CreateIssue($"This StackLayout can be replaced with {context.Platform.VerticalStackLayout}", syntax, syntax.NameSpan).AsList();
            }
            else if (orientationValue == context.Platform.StackLayoutOrientation_Horizontal)
            {
                return CreateIssue($"This StackLayout can be replaced with {context.Platform.HorizontalStackLayout}", syntax, syntax.NameSpan).AsList();
            }

            return null;
        }
    }
}
