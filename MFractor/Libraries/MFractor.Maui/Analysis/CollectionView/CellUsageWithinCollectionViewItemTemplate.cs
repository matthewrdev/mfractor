using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.CollectionView
{
    class CellUsageWithinCollectionViewItemTemplate : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects the DataTemplate within a CollectionViews ItemTemplate property and verify that it does not use a cell.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.cell_usage_within_collection_view_item_template";

        public override string Name => "Cell Usage Within CollectionView ItemTemplate";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1090";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax,
                                                           IParsedXamlDocument document,
                                                           IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.CollectionView.MetaType ))
            {
                return null;
            }

            var template = syntax.GetChildNode($"{context.Platform.CollectionView.Name}.{context.Platform.ItemSourceProperty}")?.GetChildNode(context.Platform.DataTemplate.Name);
            if (template == null)
            {
                return null;
            }

            var innerContent = template.GetChildNode(c => !XamlSyntaxHelper.IsPropertySetter(c));

            if (innerContent == null)
            {
                return null;
            }

            var contentType = context.XamlSemanticModel.GetSymbol(innerContent) as INamedTypeSymbol;
            if (contentType == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(contentType, context.Platform.Cell.MetaType))
            {
                return null;
            }

            return CreateIssue("Obsolete: Using a cell within a CollectionViews ItemTemplate property is unsupported. Remove the cell and define the items content within a data template instead.", innerContent, innerContent.Span).AsList();
        }
    }
}

