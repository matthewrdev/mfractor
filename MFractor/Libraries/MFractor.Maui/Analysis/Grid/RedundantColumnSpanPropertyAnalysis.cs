using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Grid
{
    class RedundantColumnSpanPropertyAnalysis : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.redundant_column_span_property";

        public override string Name => "Grid.ColumnSpan Usage Is Redundant";

        public override string Documentation => "This code analyser inspects usages of the `Grid.ColumnSpan` attribute and validates that the element is inside a `Grid`.";

        public override string DiagnosticId => "MF1029";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var propertyName = $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}Span";
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

            var gridType = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);

            var gridSymbol = context.XamlSemanticModel.GetSymbol(gridContainer) as INamedTypeSymbol;

            if (SymbolHelper.DerivesFrom(gridSymbol, gridType))
            {
                return null;
            }

            return CreateIssue($"This column span initialisation is redundant as the layout container is not a {context.Platform.Grid.Name}", syntax, syntax.Span).AsList();
        }
    }
}
