using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Grid
{
    class RedundantRowPropertyAnalysis : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.redundant_row_property";

        public override string Name => "Grid.Row Usage Is Redundant";

        public override string Documentation => "This code analyser inspects usages of the `Grid.Row` attribute and validates that the element is inside a `Grid`.";

        public override string DiagnosticId => "MF1030";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var propertyName = $"{context.Platform.Grid.Name}.{context.Platform.RowProperty}Span";
            if (syntax.Name.LocalName != propertyName)
            {
                return null;
            }

            var field = context.XamlSemanticModel.GetSymbol(syntax) as IFieldSymbol;
            if (field == null)
            {
                return null;
            }

            var gridContainer = syntax.Parent?.Parent;
            if (gridContainer == null)
            {
                return null;
            }

            var gridSymbol = context.XamlSemanticModel.GetSymbol(gridContainer) as INamedTypeSymbol;
            if (SymbolHelper.DerivesFrom(gridSymbol, context.Platform.Grid.MetaType))
            {
                return null;
            }

            return CreateIssue($"This Grid.Row attribute is redundant as the layout container is not a {context.Platform.Grid.MetaType}", syntax, syntax.Span).AsList();
        }
    }
}
