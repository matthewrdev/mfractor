using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Grid
{
    class ElementMissingGridPosition : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.element_missing_grid_position";

        public override string Name => "Element Missing Grid Row or Column";

        public override string Documentation => "This code analyser inspects usages of the `Grid.Row` attribute and validates that the element is inside a `Grid`.";

        public override string DiagnosticId => "MF1103";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return null;
            }

            var gridContainer = syntax.Parent;
            if (gridContainer == null)
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.VisualElement.MetaType))
            {
                return null;
            }

            var gridSymbol = context.XamlSemanticModel.GetSymbol(gridContainer) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(gridSymbol, context.Platform.Grid.MetaType))
            {
                return null;
            }

            var gridAttributes = syntax.GetAttributes(a => a.Name.FullName.StartsWith(context.Platform.Grid.Name + "."));
            if (gridAttributes.Any())
            {
                return null;
            }

            return CreateIssue($"This element is within a grid however it does not define a row or column position. Was this intended?", syntax, syntax.NameSpan).AsList();
        }
    }
}
