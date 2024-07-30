using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Grid
{
    class RedundantColumnPropertyAnalysis : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.redundant_column_property";

        public override string Name => "Grid.Column Usage Is Redundant";

        public override string Documentation => "This code analyser inspects usages of the `Grid.Column` attribute and validates that the element is inside a `Grid`.";

        public override string DiagnosticId => "MF1028";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var propertyName = $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}";
            if (syntax.Name.LocalName != propertyName)
            {
                return null;
            }

            var field = context.XamlSemanticModel.GetSymbol(syntax) as IFieldSymbol;
            if (field == null )
            {
                return null;
            }

            var container = syntax.Parent?.Parent;

            if (container == null)
            {
                return null;
            }

            var gridType = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);

            var gridSymbol = context.XamlSemanticModel.GetSymbol(container) as INamedTypeSymbol;
            if (SymbolHelper.DerivesFrom(gridSymbol, gridType))
            {
                return null;
            }

            return CreateIssue($"This {propertyName} attribute is redundant as the layout container is not a {context.Platform.Grid.MetaType}", syntax, syntax.Span).AsList();
        }
    }
}
