using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.CollectionView
{
    class CollectionViewRequiresIsGrouped : XamlCodeAnalyser
    {
        public override string Documentation => "When a `CollectionView` uses the `GroupHeaderTemplate` and/or `GroupFooterTemplate`, however, it does not set `IsGrouped` to true, this code analyser warns that the grouping may not display.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.collection_view_requires_is_grouped";

        public override string Name => "CollectionView Requires IsGrouped";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1099";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax,
                                                           IParsedXamlDocument document,
                                                           IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.CollectionView.MetaType))
            {
                return null;
            }

            var isGroupedAttribute = syntax.GetAttributeByName("IsGrouped");
            var isGrouped = isGroupedAttribute != null && isGroupedAttribute.HasValue && isGroupedAttribute.Value.Value.Equals("true", System.StringComparison.OrdinalIgnoreCase);

            if (isGrouped)
            {
                return null;
            }

            var headerTemplateNode = syntax.GetChildNode("CollectionView.GroupHeaderTemplate");
            var footerTemplateNode = syntax.GetChildNode("CollectionView.GroupFooterTemplate");
            var headerTemplateAttribute = syntax.GetAttributeByName("GroupHeaderTemplate");
            var footerTemplateAttribute = syntax.GetAttributeByName("GroupFooterTemplate");

            var usesHeader = headerTemplateNode != null || headerTemplateAttribute != null;
            var usesFooter = footerTemplateNode != null || footerTemplateAttribute != null;

            if (!usesHeader && !usesFooter)
            {
                return null;
            }

            var message = "This CollectionView uses a group footer template, however, grouping is not enabled. Please set IsGrouped to true to enable grouping.";
            if (usesHeader && usesFooter)
            {
                message = "This CollectionView uses a group header and footer template, however, grouping is not enabled. Please set IsGrouped to true to enable grouping.";
            }
            else if (usesHeader)
            {
                message = "This CollectionView uses a group header template, however, grouping is not enabled. Please set IsGrouped to true to enable grouping.";
            }

            return CreateIssue(message, syntax, syntax.OpeningTagSpan).AsList();
        }
    }
}

